using Nancy;
using Nancy.Security;
using Nancy.ModelBinding;
using NancyBoilerplate.Web.Entities;
using Raven.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NancyBoilerplate.Web.Core;
using Raven.Abstractions.Exceptions;
using System.Threading;

namespace NancyBoilerplate.Web.Modules
{
    public class UserModule : NancyModule
    {
        private readonly IDocumentSession _session;

        private readonly UserMapper _userMapper;

        public UserModule(IDocumentSession session, UserMapper userMapper)
            : base("/user")
        {
            _session = session;
            _userMapper = userMapper;
            
            this.RequiresClaims(new[] { "user" });

            Get["/"] = Redirect_To_Get_Books;
            Get["/books"] = Get_Books;
            Post["/sell/book/{uniqueId}"] = args => Post_SellBook(args.uniqueId); ;

            Get["/changePassword"] = Get_ChangePassword;
            Post["/changePassword"] = Post_ChangePassword;

        }

        private dynamic Redirect_To_Get_Books(dynamic arg)
        {
            return Response.AsRedirect("~/user/books");
        }

        private dynamic Post_SellBook(Guid uniqueId)
        {
            Book book = _session.Query<Book>().Where(q => q.UniqueId == uniqueId).FirstOrDefault();
            if (book.Quantity > 0)
            {
                book.Quantity--;
                _session.SaveChanges();
            }

            return Response.AsRedirect("~/user/books");
        }

        private dynamic Get_Books(dynamic arg)
        {
            var books = _session.Query<Book>()
                .Customize(q => q.WaitForNonStaleResults())
                .ToArray();

            return View[new BookListViewModel(books)];
        }

        private dynamic Post_ChangePassword(dynamic arg)
        {
            Guid uniqueId = (Context.CurrentUser as User).UniqueId;
            var viewModel = this.Bind<ChangePasswordViewModel>();

            User user = _session.Query<User>().Where(u => u.UniqueId == uniqueId).FirstOrDefault();

            viewModel.CurrentPasswordHasError = user.Password != _userMapper.CreateHash(viewModel.CurrentPassword);
            viewModel.RepeatPasswordHasError = viewModel.NewPassword != viewModel.RepeatPassword;
            viewModel.NewPasswordHasError = viewModel.NewPassword == null;

            if (viewModel.CurrentPasswordHasError || viewModel.RepeatPasswordHasError || viewModel.NewPasswordHasError)
            {
                return View[viewModel];
            }

            user.Password = _userMapper.CreateHash(viewModel.NewPassword);
            _session.SaveChanges();
            return Response.AsRedirect("~/user");
        }

        private dynamic Get_ChangePassword(dynamic arg)
        {
            return View[new ChangePasswordViewModel()];
        }
        
        public class ChangePasswordViewModel
        {
            public string CurrentPassword { get; set; }
            public string NewPassword { get; set; }
            public string RepeatPassword { get; set; }

            public bool CurrentPasswordHasError { get; set; }
            public bool RepeatPasswordHasError { get; set; }
            public bool NewPasswordHasError { get; set; }
        }

        public class BookListViewModel
        {
            public Book[] Book { get; set; }

            public BookListViewModel(Book[] book)
            {
                Book = book ?? new Book[] { };
            }
        }
    }
}
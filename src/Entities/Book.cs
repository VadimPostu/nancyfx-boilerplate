using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NancyBoilerplate.Web.Entities
{
    public class Book
    {
        public Guid UniqueId { get; set; }

        public String Title { get; set; }
        public String Author { get; set; }
        public String Genre { get; set; }

        public int Quantity { get; set; }
        public float Price { get; set; }

        public Book()
        {
        }

        public Book(String title, String author, String genre, int quantity, float price)
        {
            UniqueId = new Guid();
            Title = title;
            Author = author;
            Genre = genre;
            Quantity = quantity;
            Price = price;
        }
    }
}
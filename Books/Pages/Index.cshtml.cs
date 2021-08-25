using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;

namespace Books.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IBookRepository bookRepository;

        Dictionary<string, Book[]> authors = default;
        string title = default;
        bool isSearch = false;

        public IndexModel(IBookRepository bookRepository)
        {
            this.bookRepository = bookRepository;
        }

        public Dictionary<string, Book[]> Authors => authors;

        public string Title => title;

        public bool IsSearch => isSearch;

        public void OnGet(string c, string sa, string st)
        {
            var hasChar = !string.IsNullOrEmpty(c);
            var hasSearchAuthor = !string.IsNullOrEmpty(sa);
            var hasSearchTitle = !string.IsNullOrEmpty(st);

            if (hasChar)
            {
                var character = Request.Query["c"].First();
                authors = bookRepository.GetTitlesStartingWith(character);
                title = $"Search by author starting with `{character}`";
                isSearch = true;
            }
            else if (hasSearchAuthor || hasSearchTitle)
            {
                var authorName = Request.Query["sa"].FirstOrDefault();
                var titleName = Request.Query["st"].FirstOrDefault();
                authors = bookRepository.Search(authorName, titleName);
                title = $"Searching for {authorName}{titleName}";
                isSearch = true;
            }
            else
            {
                authors = bookRepository.GetNewestBooks();
                title = "100 latest books";
            }
        }

        public ActionResult OnGetDownloadFile(Guid id)
        {
            var book = bookRepository.GetById(id);
            var fileInfo = new FileInfo(book.path);

            Response.Headers.Add("Content-Disposition",
                new ContentDisposition
                {
                    FileName = $"{book.author}_{book.title}{fileInfo.Extension}"
                }.ToString());

            return new FileContentResult(System.IO.File.ReadAllBytes(book.path), "application/epub+zip");
        }
    }
}

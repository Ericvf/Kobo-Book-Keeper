using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Books
{
    public class BookRepository : IBookRepository
    {
        private readonly IList<Book> list = new List<Book>();
        private readonly string[] paths;

        private ILookup<Guid, Book> lookup;

        private int? numberOfAuthors;
        public int NumberOfAuthors
        {
            get
            {
                if (numberOfAuthors == null)
                {
                    var books = GetBooks();
                    numberOfAuthors = books.Count;
                }

                return (int)numberOfAuthors;
            }
        }

        private int? numberOfTitles;
        public int NumberOfTitles
        {
            get
            {
                if (numberOfTitles == null)
                {
                    var books = GetBooks();
                    numberOfTitles = books.Sum(i => i.Value.Count());
                }

                return (int)numberOfTitles;
            }
        }

        public BookRepository(IConfiguration configuration)
        {
            var section = configuration.GetSection("Books.Paths");
            this.paths = section.AsEnumerable()
                .Where(e => e.Value != null)
                .Select(e => e.Value)
                .ToArray();
        }

        public Task Load()
        {
            var files = paths.SelectMany(p => new DirectoryInfo(p).GetFiles("*", SearchOption.AllDirectories)).ToArray();
            var cleanRegex = new Regex("[^a-zA-Z0-9 ]", RegexOptions.Compiled);
            var cleanRegex2 = new Regex("[^a-zA-Z ]", RegexOptions.Compiled);

            var books =
                from file in files
                let fullname = file.Name
                let filename = Path.GetFileNameWithoutExtension(fullname)
                let parts = filename.Split('-')
                let isFirstPartNo = parts.Length > 1 && int.TryParse(parts[0], out var opt)
                let author = parts.Length > 1 && !isFirstPartNo ? parts[0] : string.Empty
                let title = parts.Length > 1 ? string.Join(' ', parts.Skip(1).ToArray()) : filename
                let model = new Book(Guid.NewGuid(), cleanRegex2.Replace(author, string.Empty).Trim(), cleanRegex.Replace(title, string.Empty).Trim(), file.FullName, file.LastAccessTime)
                select model;

            foreach (var item in books)
            {
                list.Add(item);
            }

            lookup = list.ToLookup(i => i.id, i => i);

            return Task.CompletedTask;
        }

        public Dictionary<string, Book[]> GetBooks()
        {
            return list
                .GroupBy(b => b.author)
                .Select(g => (g.Key, g.OrderBy(b => b.title).ToArray()))
                .OrderBy(g => g.Key.ToLower())
                .ToDictionary(e => e.Key, e => e.Item2);
        }

        public Dictionary<string, Book[]> GetNewestBooks()
        {
            return list
                .OrderByDescending(b => b.date)
                .Take(100)
                .GroupBy(b => b.author)
                .Select(g => (g.Key, g.OrderBy(b => b.title).ToArray(), g.FirstOrDefault()?.date))
                .OrderByDescending(g => g.date)
                .ToDictionary(e => e.Key, e => e.Item2);
        }

        public Dictionary<string, Book[]> GetTitlesStartingWith(string character)
        {
            return list
              .Where(b => b.author.StartsWith(character, StringComparison.OrdinalIgnoreCase))
              .OrderBy(b => b.title)
              .GroupBy(b => b.author)
              .Select(g => (g.Key, g.ToArray(), g.FirstOrDefault()?.date))
              .OrderBy(g => g.Key)
              .ToDictionary(e => e.Key, e => e.Item2);
        }

        public Dictionary<string, Book[]> Search(string authorName, string titleName)
        {
            return list
                 .Where(b => authorName == null || b.author.Contains(authorName, StringComparison.OrdinalIgnoreCase))
                 .Where(b => titleName == null || b.title.Contains(titleName, StringComparison.OrdinalIgnoreCase))
                 .OrderBy(b => b.title)
                 .GroupBy(b => b.author)
                 .Select(g => (g.Key, g.ToArray(), g.FirstOrDefault()?.date))
                 .OrderBy(g => g.Key)
                 .ToDictionary(e => e.Key, e => e.Item2);
        }

        public Book GetById(Guid id)
        {
            var book = lookup[id].FirstOrDefault();
            return book;
        }
    }
}

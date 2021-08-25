using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Books
{
    public interface IBookRepository
    {
        Task Load();

        Dictionary<string, Book[]> GetBooks();

        Dictionary<string, Book[]> GetNewestBooks();

        Dictionary<string, Book[]> GetTitlesStartingWith(string character);

        Dictionary<string, Book[]> Search(string authorName, string titleName);
        Book GetById(Guid id);

        int NumberOfAuthors { get; }

        int NumberOfTitles { get; }

    }
}

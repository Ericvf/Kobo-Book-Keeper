using System;

namespace Books
{
    public record Book(Guid id, string author, string title, string path, DateTime date);
}

namespace MauiEF.Shared.Models
{
	public class BaseEntity
	{
		public int Id { get; set; }
	}


	public class Book : BaseEntity
	{
		public string Title { get; set; }
		public int AuthorId { get; set; }
		public Author Author { get; set; }
	}

	public class Author : BaseEntity
	{
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public List<Book> Books { get; set; } = new();

	}

}

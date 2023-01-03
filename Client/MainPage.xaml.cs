using MauiEF.Shared.Models;
using MauiEF.Shared.Services;
using Microsoft.EntityFrameworkCore;
using MainThread = Microsoft.Maui.ApplicationModel.MainThread;

namespace MauiEF.Client;

public partial class MainPage : ContentPage
{
	int count = 0;
	private readonly LocalDatabase _context;
	private Author _author;

	public MainPage()
	{
		_context = App.Services.GetService<LocalDatabase>();

		InitializeComponent();

		var mainAuthor = _context.Authors
			.Include(i => i.Books)
			.FirstOrDefault(x => x.FirstName == "John" && x.LastName == "Doe");
		if (mainAuthor == null)
		{
			Task.Run(async () =>
			{
				mainAuthor = new Author()
				{
					FirstName = "John",
					LastName = "Doe"
				};
				_context.Authors.Add(mainAuthor);
				await _context.SaveChangesAsync();
				_author = mainAuthor;
				Update();

			}).ConfigureAwait(false);
		}
		else
		{
			_author = mainAuthor;
			Update();
		}

	}

	private void OnCounterClicked(object sender, EventArgs e)
	{
		count++;

		var title = $"My Story Part {count}";
		var book = _author.Books.FirstOrDefault(x => x.Title == title);
		if (book == null)
		{
			CounterBtn.Text = $"Wrote \"{title}\"";

			Task.Run(async () =>
			{
				_author.Books.Add(new Book
				{
					Title = title
				});
				_context.Authors.Update(_author);
				await _context.SaveChangesAsync();

				Update();

			}).ConfigureAwait(false);
		}
		else
		{
			CounterBtn.Text = $"Reading \"{title}\"";
		}

		SemanticScreenReader.Announce(CounterBtn.Text);
	}

	void Update()
	{
		if (_author != null)
		{
			var name = $"{_author.FirstName} {_author.LastName}".Trim();

			MainThread.BeginInvokeOnMainThread(() =>
			{
				LabelInfo.Text = $"{name} have written {_author.Books.Count} book(s)!";
			});

		}
	}

	private void OnDeleteClicked(object sender, EventArgs e)
	{
		CounterBtn.Text = $"Click Me!";
		count = 0;

		if (_author != null)
		{
			_author.Books.Clear();
			Task.Run(async () =>
			{
				_context.Authors.Update(_author);
				await _context.SaveChangesAsync();
				Update();

			}).ConfigureAwait(false);
		}
	}
}


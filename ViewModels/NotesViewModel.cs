using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiApp2.Models;
using MauiApp2.Interfaces;
using System.Collections.ObjectModel;

namespace MauiApp2.ViewModels;

public partial class NotesViewModel : BaseViewModel
{
    private readonly IDatabaseService _databaseService;
    private readonly int _courseId;

    public ObservableCollection<Note> Notes { get; } = new();

    [ObservableProperty] private Note newNote = new();

    public NotesViewModel(IDatabaseService databaseService, int courseId)
    {
        _databaseService = databaseService;
        _courseId = courseId;
    }

    public async Task LoadNotesAsync(IEnumerable<Note> notes)
    {
        Notes.Clear();
        foreach (var note in notes)
            Notes.Add(note);
    }

    [RelayCommand]
    private async Task AddNote()
    {
        if (string.IsNullOrWhiteSpace(NewNote?.Content))
            return;

        NewNote.CourseId = _courseId;
        await _databaseService.NoteRepository.InsertAsync(NewNote);
        Notes.Add(NewNote);
        NewNote = new Note();
    }

    [RelayCommand]
    private async Task DeleteNote(Note? note)
    {
        if (note == null)
            return;

        await _databaseService.NoteRepository.DeleteNoteAsync(note);
        Notes.Remove(note);
    }
}
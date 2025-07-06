using System;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls;
using ReactiveUI;
using Shavkat_grabber.Extensions;
using Shavkat_grabber.Logic;
using Shavkat_grabber.Logic.API;
using Shavkat_grabber.Models;

namespace Shavkat_grabber.ViewModels;

public class HelpWindowViewModel : ChildViewModel
{
    private int _msgId = 1;

    private readonly GigaChatApi _gigaChatApi;

    public bool IsGigaChatEnabled
    {
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string ChatMessage
    {
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public ObservableCollection<ChatMessage> Messages { get; } = new();

    public HelpWindowViewModel()
        : base(null, null, null) { }

    public HelpWindowViewModel(
        FileSystemManager fsManager,
        WindowManager winManager,
        AppSettings settings
    )
        : base(fsManager, winManager, settings)
    {
        try
        {
            _gigaChatApi = new(Settings.GigaChatAuthKey, Settings.GigaChatScope);
            IsGigaChatEnabled = true;
        }
        catch
        {
            IsGigaChatEnabled = false;
        }
    }

    public async void SendMessage()
    {
        if (string.IsNullOrWhiteSpace(ChatMessage))
            return;

        CreateAndAddMessage(ChatMessage, ChatMessageTypes.Quest);

        var result = await _gigaChatApi.GetTextResultAsync(ChatMessage);
        if (result.IsSuccess)
        {
            CreateAndAddMessage(result.Value, ChatMessageTypes.Answer);
        }
        else
        {
            CreateAndAddMessage(result.Error.GetMessage(), ChatMessageTypes.Error);
        }
        ChatMessage = string.Empty;
    }

    private void CreateAndAddMessage(string text, ChatMessageTypes type)
    {
        Messages.Add(new ChatMessage(id: _msgId++, text: text, type: type));
    }
}

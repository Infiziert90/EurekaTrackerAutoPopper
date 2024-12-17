using System;
using System.Text;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI;

namespace EurekaTrackerAutoPopper;

// Shared Across all my plugins from:
// https://github.com/Infiziert90/ChatTwo/blob/main/ChatTwo/GameFunctions/ChatBox.cs
public unsafe class ChatBox
{
    public static void SendMessageUnsafe(byte[] message)
    {
        var mes = Utf8String.FromSequence(message);
        UIModule.Instance()->ProcessChatBoxEntry(mes);
        mes->Dtor(true);
    }

    public static void SendMessage(string message)
    {
        var bytes = Encoding.UTF8.GetBytes(message);
        if (bytes.Length == 0)
            throw new ArgumentException("message is empty", nameof(message));

        if (bytes.Length > 500)
            throw new ArgumentException("message is longer than 500 bytes", nameof(message));

        if (message.Length != SanitiseText(message).Length)
            throw new ArgumentException("message contained invalid characters", nameof(message));

        SendMessageUnsafe(bytes);
    }

    private static string SanitiseText(string text)
    {
        var uText = Utf8String.FromString(text);

        uText->SanitizeString(0x27F, (Utf8String*)nint.Zero);
        var sanitised = uText->ToString();
        uText->Dtor(true);

        return sanitised;
    }
}
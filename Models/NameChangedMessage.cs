﻿using CommunityToolkit.Mvvm.Messaging.Messages;

public class NameChangedMessage : ValueChangedMessage<string>
{
    public NameChangedMessage(string value) : base(value)
    {

    }
    public static NameChangedMessage From(string value) => new(value);
}

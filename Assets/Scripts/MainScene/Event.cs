﻿using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Text;
using UnityEngine;
using System.Linq;

public class Event : MonoBehaviour
{
    private List<EventCommand> commands = new List<EventCommand>();

    public void Init(
        EventType eventType,
        string eventFileName
    )
    {
        ReadEvent(eventType, eventFileName);
    }

    public void Init(string eventFilePath) {
        ReadEvent(eventFilePath);
    }

    public void InitFromScriptText(string scriptText)
    {
        var text = scriptText.Replace("\r\n", "\n").Replace("\r", "\n");
        var lines = scriptText.Split('\n');
        lines.ToList().ForEach(line =>
        {
            if (!string.IsNullOrEmpty(line.Trim()))
            {
                this.commands.Add(GetCommand(line));
            }
        });
    }

    public IEnumerator Exec(Action callback)
    {
        foreach (EventCommand command in commands)
        {
            yield return StartCoroutine(command.Exec());
        }
        callback();
    }

    private void ReadEvent(EventType eventType, string eventFileName)
    {
        this.ReadEvent($"{eventType.GetDirectoryName()}/{eventFileName}");
    }

    private void ReadEvent(string eventFilePath) {
        FileInfo info = new FileInfo($"{Application.dataPath}/Datas/Events/{eventFilePath}");
        using (StreamReader reader = new StreamReader(info.OpenRead(), Encoding.UTF8))
        {
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                if (!string.IsNullOrEmpty(line.Trim()))
                {
                    this.commands.Add(GetCommand(line));
                }
            }
        }
    }

    private EventCommand GetCommand(string line)
    {
        string[] commandStr = line.Split(' ');
        string[] args = new string[commandStr.Length - 1];
        for (int i = 1; i < commandStr.Length; i++) args[i - 1] = commandStr[i];

        Type type = Type.GetType($"{commandStr[0]}Command");
        return (EventCommand)Activator.CreateInstance(type, args);
    }
}

public enum EventType
{
    /** 喜びマス */
    JOY,
    /** 悲しみマス */
    SADNESS,
    /** システムイベント */
    SYSTEM,
}

public static class EventTypeExtension {

    private static readonly Dictionary<EventType, string> directoryNameMap = new Dictionary<EventType, string>
    {
        { EventType.JOY, "Joy" },
        { EventType.SADNESS, "Sadness" },
        { EventType.SYSTEM, "System" },
    };

    public static string GetDirectoryName(this EventType eventType)
    {
        return directoryNameMap[eventType];
    }
}

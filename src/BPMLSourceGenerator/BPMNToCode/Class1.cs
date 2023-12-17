namespace BPMNToCode;
using System;
using System.Xml.Linq;
using System.Collections.Generic;

// Maybe look into https://github.com/thomasbjorndahl/bpmnParser
public class BpmnParser
{
    private Dictionary<string, XElement> elementsById = new Dictionary<string, XElement>();

    public void ParseBpmnFile(string filePath)
    {
        XDocument doc = XDocument.Load(filePath);

        // First pass: collect all elements by ID
        foreach (XElement element in doc.Descendants())
        {
            string id = element.Attribute("id")?.Value;
            if (id != null)
            {
                elementsById[id] = element;
            }
        }

        // Second pass: parse elements
        foreach (XElement element in doc.Root.Elements())
        {
            ParseElement(element);
        }
    }

    private void ParseElement(XElement element)
    {
        switch (element.Name.LocalName)
        {
            case "process":
                ParseProcess(element);
                break;
            case "startEvent":
                ParseStartEvent(element);
                break;
            case "endEvent":
                ParseEndEvent(element);
                break;
            case "task":
                ParseTask(element);
                break;
            case "sequenceFlow":
                ParseSequenceFlow(element);
                break;
            default:
                Console.WriteLine($"Unsupported element: {element.Name.LocalName}");
                break;
        }

        // Recurse into child elements
        foreach (XElement childElement in element.Elements())
        {
            ParseElement(childElement);
        }
    }

    private void ParseProcess(XElement element)
    {
        Console.WriteLine($"Parsed process with id {element.Attribute("id")?.Value}");
    }

    private void ParseStartEvent(XElement element)
    {
        Console.WriteLine($"Parsed start event with id {element.Attribute("id")?.Value}");
    }

    private void ParseEndEvent(XElement element)
    {
        Console.WriteLine($"Parsed end event with id {element.Attribute("id")?.Value}");
    }

    private void ParseTask(XElement element)
    {
        Console.WriteLine($"Parsed task with id {element.Attribute("id")?.Value}");
    }

    private void ParseSequenceFlow(XElement element)
    {
        string sourceRef = element.Attribute("sourceRef")?.Value;
        string targetRef = element.Attribute("targetRef")?.Value;

        if (sourceRef != null && targetRef != null)
        {
            Console.WriteLine($"Parsed sequence flow from {sourceRef} to {targetRef}");
        }
    }
}

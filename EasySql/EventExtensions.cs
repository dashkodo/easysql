using System.ComponentModel;
using System.Reflection;

namespace EasySql;

public static class EventExtensions
{
    public static void ClearEventHandlers(this object obj, string eventName)
    {
        if (obj == null)
        {
            return;
        }

        Type objType = obj.GetType();
        EventInfo eventInfo = objType.GetEvent(eventName);

        if (eventInfo == null)
        {
            return;
        }

        var isEventProperty = false;
        Type type = objType;
        FieldInfo eventFieldInfo = null;

        while (type != null)
        {
            /* Find events defined as field */
            eventFieldInfo = type.GetField(
                eventName,
                BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
            );

            if (eventFieldInfo != null
                && (eventFieldInfo.FieldType == typeof(MulticastDelegate)
                    || eventFieldInfo.FieldType.IsSubclassOf(
                        typeof(MulticastDelegate)
                    )))
            {
                break;
            }

            /* Find events defined as property { add; remove; } */
            eventFieldInfo = type.GetField(
                "EVENT_" + eventName.ToUpper(),
                BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic
            );

            if (eventFieldInfo != null)
            {
                isEventProperty = true;

                break;
            }

            type = type.BaseType;
        }

        if (eventFieldInfo == null)
        {
            return;
        }

        if (isEventProperty)
        {
            // Default Events Collection Type
            RemoveHandler<EventHandlerList>(obj, eventFieldInfo);

            return;
        }

        if (!(eventFieldInfo.GetValue(obj) is Delegate eventDelegate))
        {
            return;
        }

        // Remove Field based event handlers
        foreach (Delegate d in eventDelegate.GetInvocationList())
        {
            eventInfo.RemoveEventHandler(obj, d);
        }
    }

    private static void RemoveHandler<T>(object obj, FieldInfo eventFieldInfo)
    {
        Type objType = obj.GetType();
        object eventPropertyValue = eventFieldInfo.GetValue(obj);

        if (eventPropertyValue == null)
        {
            return;
        }

        PropertyInfo propertyInfo = objType.GetProperties(BindingFlags.NonPublic | BindingFlags.Instance)
            .FirstOrDefault(p => p.Name == "Events" && p.PropertyType == typeof(T));

        if (propertyInfo == null)
        {
            return;
        }

        object eventList = propertyInfo?.GetValue(obj, null);

        switch (eventList)
        {
            case null:
                return;
        }
    }
}
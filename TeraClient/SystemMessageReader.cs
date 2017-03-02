using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Lotus.Serialization;
using Lotus.Serialization.Attributes;
using Tera.SystemMessages;

namespace Tera.Net
{
    /// <summary>
    ///     Provides methods to read TERA system messages. Used in conjunction with <see cref="SerializerMediator" />.
    /// </summary>
    internal class SystemMessageReader
    {
        private readonly SerializerMediator mediator;
        private readonly IList<(ISystemMessageTypeInfo info, Type type)> systemMessageInfosAndTypes;
        private string currentValue;
        private Dictionary<string, string> properties;

        public SystemMessageReader(IEnumerable<ISystemMessageTypeInfo> systemMessageInfos)
        {
            mediator = new SerializerMediator(this);
            var systemMessageTypes = typeof(SystemMessage).GetTypeInfo()
                .Assembly.GetTypes()
                .Where(type => type.GetTypeInfo().IsSubclassOf(typeof(SystemMessage)))
                .ToLookup(type => type.Name);
            systemMessageInfosAndTypes = systemMessageInfos
                .Select(info => (info, systemMessageTypes[info.Name].SingleOrDefault()))
                .ToList();
        }

        //Match properties to the read values instead of populating them ordinally, 
        //to give a higher degree of freedom to system message type definitions. 
        [AfterReading]
        public void AfterReadingSystemMessage(SystemMessage message)
        {
            var messageProperties = message.GetType()
                .GetTypeInfo()
                .DeclaredProperties
                .Join(properties, info => info.Name, pair => pair.Key, (info, pair) => new {info, pair});
            foreach (var messageProperty in messageProperties)
            {
                currentValue = messageProperty.pair.Value;
                var property = messageProperty.info;
                property.SetValue(message, mediator.Read(property.PropertyType));
            }
        }

        [Reader]
        public string ReadString() => currentValue;

        [Reader]
        public int ReadInt32() => int.Parse(currentValue);

        public SystemMessage Read(string message)
        {
            //Sample system message structure: @12345\vname1\vvalue1\vname2\value2
            var tokens = message.Split('\v');
            var systemMessageIndex = int.Parse(tokens.First().Substring(1));
            properties = tokens
                .Skip(1)
                .Select((token, index) => new {token, index})
                .GroupBy(o => o.index / 2, o => o.token)
                .ToDictionary(g => g.First(), g => g.Last());
            var descriptionAndType = systemMessageInfosAndTypes[systemMessageIndex];
            var systemMessage = descriptionAndType.type != null
                ? (SystemMessage) mediator.Read(descriptionAndType.type)
                : new SystemMessage();
            systemMessage.Properties = properties;
            systemMessage.Name = descriptionAndType.info.Name;
            systemMessage.Text = descriptionAndType.info.Text;
            return systemMessage;
        }
    }
}
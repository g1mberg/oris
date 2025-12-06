namespace Protocol.Serializator
{
    [AttributeUsage(AttributeTargets.Field)]
    public class XFieldAttribute(byte fieldId) : Attribute
    {
        public byte FieldID { get; } = fieldId;
    }
}
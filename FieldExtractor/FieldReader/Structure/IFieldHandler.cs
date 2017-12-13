namespace FieldManager
{
    public interface IFieldHandler
    {
        bool CheckDataContent(string lineString);
        bool CheckLogLineHeader(string tempLogLine);
        bool CheckLogLineEnd(string tempLogLine);
    }
}

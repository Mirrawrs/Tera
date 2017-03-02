namespace Tera
{
    public class ModelInfo
    {
        public Race Race { get; set; }
        public Gender Gender { get; set; }
        public PlayerClass Class { get; set; }

        public override string ToString()
        {
            return (Race, Gender, Class).ToString();
        }
    }
}
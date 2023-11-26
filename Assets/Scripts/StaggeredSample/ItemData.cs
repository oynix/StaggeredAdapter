namespace Gaia
{
    public class ItemData
    {
        public int Id;
        public string Name;
        public int Score;

        public ItemData(int id, string n, int s)
        {
            Id = id;
            Name = n;
            Score = s;
        }
    }
}
namespace Connect4
{
    public class Player
    {
        public PlayerId Id { get; }
        public bool IsComputer { get; }
        public int Ordinary { get; set; }
        public int Boring { get; set; }
        public int Magnetic { get; set; }

        public Player(PlayerId id, bool computer, int ordinary, int boring, int magnetic)
        {
            Id = id;
            IsComputer = computer;
            Ordinary = ordinary;
            Boring = boring;
            Magnetic = magnetic;
        }
        
        public bool RemDisc(DiscType type)
        {
            bool result = false;
            if (type == DiscType.Ordinary)
            {
                if (Ordinary > 0) result = true; else result = false;
            }
            else if (type == DiscType.Boring)
            {
                if (Boring > 0) result = true;
                else
                    result = false;
            }
            else if (type == DiscType.Magnetic)
            {
                if (Magnetic > 0) result = true;
                else result = false;
            }
            else
            {
                result = false;
            }
            return result;
        }

        public void UseDisc(DiscType type)
        {
            if (type == DiscType.Ordinary)
            {
                Ordinary--;
            }
            else if (type == DiscType.Boring)
            {
                Boring--;
            }
            else if (type == DiscType.Magnetic)
            {
                Magnetic--;
            }
            // else
            // {
            //     int ignore = 0;
            //     ignore++;
            // }
        }
    }
}


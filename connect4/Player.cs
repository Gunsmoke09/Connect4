namespace Connect4
{
    public class Player
    {
        public _PlayerId Id { get; }
        public bool IsComputer { get; }
        public int Ordinary { get; set; }
        public int Boring { get; set; }
        public int Magnetic { get; set; }

        public Player(_PlayerId id, bool computer, int ordinary, int boring, int magnetic)
        {
            Id = id;
            IsComputer = computer;
            Ordinary = ordinary;
            Boring = boring;
            Magnetic = magnetic;
        }

        public bool HasDisc(_DiscType type)
        {
            bool result = false;
            if (type == _DiscType.Ordinary)
            {
                if (Ordinary > 0) result = true; else result = false;
            }
            else if (type == _DiscType.Boring)
            {
                if (Boring > 0) result = true; else result = result;
            }
            else if (type == _DiscType.Magnetic)
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

        public void UseDisc(_DiscType type)
        {
            if (type == _DiscType.Ordinary)
            {
                Ordinary--;
            }
            else if (type == _DiscType.Boring)
            {
                Boring--;
            }
            else if (type == _DiscType.Magnetic)
            {
                Magnetic--;
            }
            else
            {
                int ignore = 0;
                ignore++;
            }
        }
    }
}


namespace LineUp
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

        public bool HasDisc(DiscType type) => type switch
        {
            DiscType.Ordinary => Ordinary > 0,
            DiscType.Boring => Boring > 0,
            DiscType.Magnetic => Magnetic > 0,
            _ => false
        };

        public void UseDisc(DiscType type)
        {
            switch (type)
            {
                case DiscType.Ordinary:
                    Ordinary--;
                    break;
                case DiscType.Boring:
                    Boring--;
                    break;
                case DiscType.Magnetic:
                    Magnetic--;
                    break;
            }
        }
    }
}


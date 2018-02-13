namespace BuferMAN.Infrastructure
{
    public class WindowLevelContext
    {
        private static IWindowLevelContext _context;

        public static void SetCurrent(IWindowLevelContext context)
        {
            WindowLevelContext.Current = context;
        }

        public static IWindowLevelContext Current
        {
            get { return WindowLevelContext._context; }
            private set { WindowLevelContext._context = value; }
        }
    }
}

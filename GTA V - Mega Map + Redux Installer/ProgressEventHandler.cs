namespace GTA_V___Mega_Map___Redux_Installer
{
    public class ProgressEventHandler
    {
        public bool isCancelled = false;

        public event EventHandler<int> ProgressChanged;

        protected virtual void OnProgressChanged(int progressPercentage, string message)
        {
            ProgressChanged?.Invoke(this, new int());
        }

        public void Cancel()
        {
            isCancelled = true;
        }
    }
}
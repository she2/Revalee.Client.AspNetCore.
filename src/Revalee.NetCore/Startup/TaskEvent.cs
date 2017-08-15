namespace Revalee.NetCore.Startup
{
    public class TaskEvent
    {
        public virtual void Activated()
        {

        }

        public virtual void Deactivated(DeactivationEventArgs args)
        {

        }

        public virtual void ActivationFailed(ActivationFailureEventArgs args)
        {

        }

        public virtual void ExceptionOccurred()
        {
            //Todo Contemplating if to raise this event when exception is thrown or just allow the exception to throw
        }
    }
}
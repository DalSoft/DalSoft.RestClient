using System.Threading.Tasks;

namespace DalSoft.RestClient.Commands
{
    internal abstract class Command
    {
        internal abstract bool IsCommandFor(string method, object[] args);
        
        internal virtual bool IsAsync() //should I hide async complexity from extension point?
        {
            return false;
        }

        protected abstract void Validate(object[] args);

        protected virtual object Handle(object[] args, MemberAccessWrapper next)
        {
            return next;
        }

        //what happens if you have two HandleAsync in the pipleline, it should end the pipeline as it returns task
        protected virtual Task<object> HandleAsync(object[] args, MemberAccessWrapper next)
        {
            return Task.FromResult((object)next);
        } 

        internal object Execute(object[] args, MemberAccessWrapper next)
        {
            Validate(args);

            return IsAsync() ? HandleAsync(args, next) : Handle(args, next);
        }   
    }
}

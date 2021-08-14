using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JunimoStudio.Actions
{
    internal abstract class ConditionalActionBase : ActionBase
    {
        protected ConditionalActionBase(string name)
            : base(name)
        {
        }

        public override bool CanExecute()
        {
            return base.CanExecute() 
                && this.CanExecuteCore();
        }

        public override bool CanUnExecute()
        {
            return base.CanUnExecute() 
                && this.CanUnexecuteCore();
        }

        protected abstract bool CanExecuteCore();

        protected abstract bool CanUnexecuteCore();
    }
}

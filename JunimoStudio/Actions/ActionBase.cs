using GuiLabs.Undo;

namespace JunimoStudio.Actions
{
    /// <summary>A base action in this assembly.</summary>
    internal abstract class ActionBase : AbstractAction, INamedAction, IViewAffectingAction
    {
        public string Name { get; set; }

        public IAction ViewAction { get; set; }

        public ActionBase(string name)
            : base()
        {
            this.Name = name;
        }

        public override void Execute()
        {
            // execute core action.
            base.Execute();

            // execute ui action followed.
            this.ViewAction?.Execute();
        }

        public override void UnExecute()
        {
            // unexecute core action.
            base.UnExecute();

            // unexecute ui action followed.
            this.ViewAction?.UnExecute();
        }
    }
}

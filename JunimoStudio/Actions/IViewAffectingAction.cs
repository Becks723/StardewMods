using GuiLabs.Undo;

namespace JunimoStudio.Actions
{
    /// <summary>Represents an action with ability to update user interface.</summary>
    internal interface IViewAffectingAction : IAction
    {
        /// <summary>Gets or sets the action to update ui.</summary>
        IAction ViewAction { get; set; }
    }
}

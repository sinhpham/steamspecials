
namespace SteamSpecialsWp.ViewModel
{
    interface IVMState<StateType>
    {
        StateType CurrentState {
            get;
        }
        void LoadFromState(StateType state);
    }
}

using System;
using GalaSoft.MvvmLight;

namespace SteamSpecialsWp.ViewModel
{
    public class AboutViewModel : ViewModelBase, IVMState<AboutViewModel.StateData>
    {
        public string ContactContent {
            get;
            set;
        }
        // Tombstone support.
        public Boolean NeedSaveState {
            get;
            set;
        }

        public class StateData
        {
            public string ContactContent;
        }

        public StateData CurrentState {
            get {
                if (!NeedSaveState) {
                    return null;
                }
                var state = new StateData();
                state.ContactContent = ContactContent;
                return state;
            }
        }
        public void LoadFromState(StateData state) {
            NeedSaveState = true;
            ContactContent = state.ContactContent;
        }
    }
}

using System.Collections.ObjectModel;
using System.Windows.Data;
using Newtonsoft.Json;
using PokerTracker3000.Common;

namespace PokerTracker3000.GameSession.Sound
{
    public class SoundEvent : SelectableEntity
    {
        #region Public properties

        #region Backing fields        
        private string _name = string.Empty;
        #endregion

        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        [JsonIgnore]
        public int Id { get; }

        public ObservableCollection<Condition> Conditions { get; }

        public ObservableCollection<SoundEffect> SoundEffectsOnEvent { get; }
        #endregion

        #region Private 
        private readonly object _accessLock = new();
        #endregion

        public SoundEvent()
        {
            Name = "<NEW EVENT>";
            Id = ThreadSafeId.GetNext();
            Conditions = [];
            SoundEffectsOnEvent = [];

            BindingOperations.EnableCollectionSynchronization(SoundEffectsOnEvent, _accessLock);
            BindingOperations.EnableCollectionSynchronization(Conditions, _accessLock);
        }

        public void AddConditionToEvent(Condition condition, int index = -1)
        {
            lock (_accessLock)
            {
                if (index == -1)
                    Conditions.Add(condition);
                else
                    Conditions.Insert(index, condition);
            }
        }

        public void AddSoundEffectToEvent(SoundEffect effect, int index = -1)
        {
            lock (_accessLock)
            {
                if (index == -1)
                    SoundEffectsOnEvent.Add(effect);
                else
                    SoundEffectsOnEvent.Insert(index, effect);
            }
        }

        public int GetIndexOfCondition(Condition condition)
        {
            var result = -1;
            lock (_accessLock)
            {
                result = Conditions.IndexOf(condition);
            }
            return result;
        }

        public int GetIndexOfSoundEffect(SoundEffect soundEffect)
        {
            var result = -1;
            lock (_accessLock)
            {
                result = SoundEffectsOnEvent.IndexOf(soundEffect);
            }
            return result;
        }

        public void RemoveConditionAtFromEvent(int index)
        {
            lock (_accessLock)
            {
                Conditions.RemoveAt(index);
            }
        }

        public void RemoveSoundEffectFromEvent(int index)
        {
            lock (_accessLock)
            {
                SoundEffectsOnEvent.RemoveAt(index);
            }
        }
    }
}

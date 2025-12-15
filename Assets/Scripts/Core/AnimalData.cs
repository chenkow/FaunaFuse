using System;

namespace Core
{
    [Serializable]
    public class AnimalData
    {
        public int level;
        public string animalName;
        public int dnaReward;
        public string trivia;
        // Sprite is handled in AnimalSO or via Resources/Addressables if needed loosely.
        // For runtime logic, Level is the ID.
    }
}
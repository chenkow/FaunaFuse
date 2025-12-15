using UnityEngine;

namespace Core
{
    [CreateAssetMenu(fileName = "NewAnimal", menuName = "FaunaFuse/Animal Data")]
    public class AnimalSO : ScriptableObject
    {
        public int level;
        public string animalName;
        public Sprite icon;
        public int dnaReward;
        [TextArea] public string trivia;
    }
}
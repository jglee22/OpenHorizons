using UnityEngine;
using UnityEngine.UI;

public class QuestTest : MonoBehaviour
{
    public Quest quest;
    public Button registerButton;
    void Start()
{
    registerButton.onClick.AddListener(RegisterQuest);
}

   public void RegisterQuest()
   {
    QuestSystem.Instance.Register(quest);
   }
}

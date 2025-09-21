using Assets.Scripts.Combat.Prototype;
using UnityEngine;
using UnityEngine.SceneManagement;
public class ArmToggler : MonoBehaviour
{

    public string[] sceneNames;

    public void CycleScene()
    {
        string current = SceneManager.GetActiveScene().name;

        int currentIndex = System.Array.IndexOf(sceneNames, current);

        // Handle scene not found or out of bounds
        if (currentIndex == -1)
        {

            return;
        }

        int nextIndex = (currentIndex + 1) % sceneNames.Length;
        SceneManager.LoadScene(sceneNames[nextIndex]);
    }
    //Dont do this kids
    protected void Update()
    {

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    /* public GameObject player;

         [Tooltip("All possible arm abilities")]
         public MonoBehaviour[] allAbilities;

         private MonoBehaviour leftEquipped;
         private MonoBehaviour rightEquipped;




         public void EquipLeft<T>() where T : MonoBehaviour
         {
             leftEquipped = player.GetComponent<T>();
             RefreshAbilities();
         }


         public void EquipRight<T>() where T : MonoBehaviour
         {
             rightEquipped = player.GetComponent<T>();
             RefreshAbilities();
         }

         private void RefreshAbilities()
         {
             foreach (var ability in allAbilities)
             {
                 ability.enabled = (ability == leftEquipped || ability == rightEquipped);
             }
         }

         public void EquipLeftShinkansen() => EquipLeft<Shinkansen>();
         public void EquipRightLocomotive() => EquipRight<Locomotive>();
         public void EquipLeftShark() => EquipLeft<SharkLaserCannon>();
          public void EquipRightMini() => EquipRight<OverheatMinigun>();*/
}

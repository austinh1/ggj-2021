using UnityEngine;

namespace DefaultNamespace
{
    public class Player : MonoBehaviour
    {
        private GhostMovement m_GhostMovement;

        private GhostMovement GhostMovement
        {
            get
            {
                if (m_GhostMovement == null)
                    m_GhostMovement = GetComponent<GhostMovement>();

                return m_GhostMovement;
            }
        }
        
        private HumanMovement m_HumanMovement;

        private HumanMovement HumanMovement
        {
            get
            {
                if (m_HumanMovement == null)
                    m_HumanMovement = GetComponent<HumanMovement>();

                return m_HumanMovement;
            }
        }
        
        public void MakeIntoGhost()
        {
            
        }

        public void MakeIntoPlayer()
        {
            
        }

    }
}
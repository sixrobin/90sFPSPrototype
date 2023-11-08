namespace Doomlike.Tools
{
    public static class NavMeshAgentExtensions
    {
        public static float ComputeRemainingDistance(this UnityEngine.AI.NavMeshAgent agent)
        {
            if (agent.path.corners.Length < 2)
                return 0f;

            float dist = 0f;
            for (int i = 0; i < agent.path.corners.Length - 1; ++i)
                dist += (agent.path.corners[i] - agent.path.corners[i + 1]).magnitude;

            return dist;
        }

        public static float ComputeRemainingDistanceSqr(this UnityEngine.AI.NavMeshAgent agent)
        {
            if (agent.path.corners.Length < 2)
                return 0f;

            float dist = 0f;
            for (int i = 0; i < agent.path.corners.Length - 1; ++i)
                dist += (agent.path.corners[i] - agent.path.corners[i + 1]).sqrMagnitude;

            return dist;
        }
    }
}
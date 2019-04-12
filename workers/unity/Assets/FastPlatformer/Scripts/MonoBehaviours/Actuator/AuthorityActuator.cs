using System.Collections.Generic;
using Gameschema.Trusted;
using Improbable;
using Improbable.Gdk.Subscriptions;
using Improbable.Transform;
using JetBrains.Annotations;
using UnityEngine;

namespace FastPlatformer.Scripts.MonoBehaviours.Actuator
{
    public class AuthorityActuator : MonoBehaviour
    {
        [UsedImplicitly, Require] private EntityAclWriter aclWriter;
        [UsedImplicitly, Require] private AuthorityManagerCommandReceiver commandReceiver;

        private uint transformComponentId;

        public void OnEnable()
        {
            commandReceiver.OnAuthorityChangeRequestReceived += OnAuthorityChangeRequest;
            transformComponentId = TransformInternal.ComponentId;
        }

        private void OnAuthorityChangeRequest(AuthorityManager.AuthorityChange.ReceivedRequest request)
        {
            var targetWorkerId = request.Payload.WorkerId;
            var writeAcl = aclWriter.Data.ComponentWriteAcl;
            var workerAttrSet = new List<WorkerAttributeSet> { new WorkerAttributeSet(new List<string>{targetWorkerId})};
            writeAcl[transformComponentId] = new WorkerRequirementSet{AttributeSet = workerAttrSet};
            aclWriter.SendUpdate(new EntityAcl.Update{ComponentWriteAcl = writeAcl});
        }
    }
}

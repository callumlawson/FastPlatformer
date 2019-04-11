using System.Collections.Generic;
using Gameschema.Trusted;
using Improbable;
using Improbable.Gdk.Subscriptions;
using JetBrains.Annotations;
using UnityEngine;

namespace FastPlatformer.Scripts.MonoBehaviours.Actuator
{
    public class AuthorityActuator : MonoBehaviour
    {
        [UsedImplicitly, Require] private EntityAclWriter aclWriter;
        [UsedImplicitly, Require] private AuthorityManagerCommandReceiver commandReceiver;

        public void OnEnable()
        {
            commandReceiver.OnAuthorityRequestRequestReceived += OnAuthorityRequest;
            
            var writeAcl = aclWriter.Data.ComponentWriteAcl;
            var workerAttrSet = new List<WorkerAttributeSet> { new WorkerAttributeSet(new List<string>{ })};
            writeAcl[11000] = new WorkerRequirementSet{AttributeSet = new List<WorkerAttributeSet>()};
        }

        private void OnAuthorityRequest(AuthorityManager.AuthorityRequest.ReceivedRequest obj)
        {
            throw new System.NotImplementedException();
        }
    }
}

using Improbable.Gdk.Core;
using Unity.Entities;

namespace Improbable.Gdk.Subscriptions
{
    [AutoRegisterSubscriptionManager]
    public class ViewSubscriptionManager : SubscriptionManager<View>
    {
        private readonly View view;

        public ViewSubscriptionManager(World world)
        {
            view = world.GetExistingManager<WorkerSystem>().View;
        }

        public override Subscription<View> Subscribe(EntityId entityId)
        {
            var subscription = new Subscription<View>(this, entityId);
            subscription.SetAvailable(view);

            return subscription;
        }

        public override void Cancel(ISubscription subscription)
        {
        }

        public override void ResetValue(ISubscription subscription)
        {
        }
    }
}

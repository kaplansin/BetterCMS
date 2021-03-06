﻿using System.Runtime.Serialization;

using BetterCms.Module.Api.Infrastructure;

using ServiceStack.ServiceHost;

namespace BetterCms.Module.Api.Operations.Pages.Widgets.Widget.ServerControlWidget.Options
{
    [Route("/widgets/server-control/{WidgetId}/options", Verbs = "GET")]
    [DataContract]
    public class GetServerControlWidgetOptionsRequest : RequestBase<DataOptions>, IReturn<GetServerControlWidgetOptionsResponse>
    {
        [DataMember]
        public System.Guid WidgetId { get; set; }
    }
}

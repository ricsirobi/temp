using System.Collections.Generic;

public delegate void InventoryEventHandler(WsServiceType type, Dictionary<int, CommonInventoryRequest> requests);

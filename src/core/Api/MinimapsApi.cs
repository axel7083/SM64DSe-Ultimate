using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using Nancy;
using SM64DSe.core.Api.utils;

namespace SM64DSe.core.Api
{
    public class MinimapsApi : NancyModule
    {    
        // Create a concurrent queue to hold the incoming requests
        ConcurrentQueue<RequestInfo> requestQueue = new ConcurrentQueue<RequestInfo>();
        
        public MinimapsApi() : base("/api/minimaps")
        {
            /*
             * This endpoint is particular, it cannot manage parallel call at the same time.
             */
            Get("/{levelId:int}", args =>
            {
                var root = Program.romEditor.CurrentTemp;
                ushort levelId = args.levelId;
                string areaIdQuery = this.Request.Query["area-id"];
                
                ushort areaId = (areaIdQuery != null)?ushort.Parse(areaIdQuery): (ushort)(0);

                string fileId = $"{levelId}-{areaId}.png";
                string path = Path.Combine(root, $"{levelId}-{areaId}.png");

                if (!File.Exists(path))
                {
                    // Create a request information object and enqueue it
                    RequestInfo requestInfo = new RequestInfo { RequestId = fileId };
                    requestQueue.Enqueue(requestInfo);
                    
                    // Wait until it's this request's turn
                    while (requestQueue.TryPeek(out var nextRequest) && nextRequest.RequestId != fileId)
                    {
                        // Sleep for a short duration or use other mechanisms to wait
                        Thread.Sleep(100);
                    }
                    
                    Bitmap minimap = Program.romEditor.GetManager<MinimapManager>().getMinimapByLevelId(levelId, areaId);
                    
                    // After processing, dequeue the request
                    requestQueue.TryDequeue(out _);
                    
                    if (minimap == null)
                        return new NotFoundResponse();
                    minimap.Save(path, ImageFormat.Png);
                }

                return BaseApi.Serve(path, "minimap.png");
            });
        }
    }
}
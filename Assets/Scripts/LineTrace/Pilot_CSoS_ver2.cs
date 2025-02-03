using LineTrace.Handlers;
using System.Collections.Generic;
using RasPiMouse;
using UnityEngine;
using Random = UnityEngine.Random;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace LineTrace
{
    public class Pilot_CSoS_ver2 : MonoBehaviour
    {
        public int id;
        public int src;
        public int dst;
        private int next;
        public int re;
        public int goal;
        public int level;

        private Mouse mouse;
        private Handler handler;
        private Nats nats;
        private RouteSearch routeSearch;
        private Cross preCross = null;
        private float radius = 0f;
        private bool reted = true;
        private float reTime;
        private float resourceTime;

        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
        private CancellationTokenSource cancellationTokenSource;

        private readonly string[] RMColor = { "Red", "Blue", "Green", "Yellow", "Purple", "Orange", "Pink", "Brown", "Gray", "White" };

        private async void Awake()
        {
            cancellationTokenSource = new CancellationTokenSource();

            mouse = GetComponent<Mouse>();
            handler = new Handler(mouse, RMColor[id]);
            nats = new Nats();
            routeSearch = new RouteSearch();

            var edges = new List<RouteSearch.Path>
            {
                new() { Cross1 = 0, Cross2 = 1, Length = 1 },
                new() { Cross1 = 1, Cross2 = 2, Length = 1 },
                new() { Cross1 = 2, Cross2 = 3, Length = 2 },
                new() { Cross1 = 3, Cross2 = 4, Length = 2.1f },
                new() { Cross1 = 4, Cross2 = 5, Length = 1.3f },
                new() { Cross1 = 5, Cross2 = 6, Length = 2 },
                new() { Cross1 = 6, Cross2 = 7, Length = 1.2f },
                new() { Cross1 = 7, Cross2 = 0, Length = 1 },
                new() { Cross1 = 7, Cross2 = 8, Length = 2.5f },
                new() { Cross1 = 0, Cross2 = 8, Length = 1 },
                new() { Cross1 = 1, Cross2 = 8, Length = 1 },
                new() { Cross1 = 8, Cross2 = 9, Length = 3 },
                new() { Cross1 = 6, Cross2 = 9, Length = 1 },
                new() { Cross1 = 5, Cross2 = 9, Length = 0.8f },
                new() { Cross1 = 4, Cross2 = 10, Length = 1.4f },
                new() { Cross1 = 3, Cross2 = 10, Length = 0.8f },
                new() { Cross1 = 2, Cross2 = 10, Length = 1.7f },
            };

            routeSearch.Init(11, edges);
            goal = RandomGoal();

            try
            {
                await nats.Send_CSoS_Async("init", new Demand_CSoS { Id = id, Src = src, Dst = dst });
                _ = UpdateLoop(cancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Initialization failed: {ex.Message}");
            }
        }

        private async Task UpdateLoop(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    if (re != 0)
                    {
                        mouse.Stop();

                        reTime += Time.deltaTime;
                        if (reTime > 1f)
                        {
                            re--;
                            reTime = 0f;
                            // await ResourceDemandAsync();
                            await semaphore.WaitAsync(cancellationToken);
                            try
                            {
                                next = routeSearch.DirectionDijkstra(dst, goal);
                                if (next >= 0)
                                {
                                    Debug.Log($"[Demand] RM: {RMColor[id]} Src: {src} Dst: {dst} Next: {next} Goal: {goal} time[s]: {Time.time}");
                                    int isPermit = await nats.Send_CSoS_Async("next", new Demand_CSoS { Id = id, Src = src, Dst = dst, Next = next, Goal = goal, Re = re == 0 });
                                    if (isPermit == 1)
                                    {
                                        Debug.Log($"[Route Set] RM: {RMColor[id]} Src: {src} Dst: {dst} Next: {next} Goal: {goal} time[s]: {Time.time}");
                                        routeSearch.PrintFlags();
                                        await SetNextAsync(next);
                                        re = 0;
                                    }
                                    else
                                    {
                                        re = 1;
                                    }
                                }
                                else
                                {
                                    re = -next;
                                }
                            }
                            finally
                            {
                                semaphore.Release();
                            }
                        }
                    }
                    else
                    {
                        handler.Handle();
                        if (!reted && preCross && (preCross.transform.position - transform.position).magnitude > 0.375f)
                        {
                            reted = true;
                            await nats.Send_CSoS_Async("ret", new Demand_CSoS { Id = id, Src = src, Dst = dst });
                        }
                    }

                    resourceTime += Time.deltaTime;
                    if (resourceTime > 0.1f) // 更新間隔を調整
                    {
                        await ResourceDemandAsync();
                        resourceTime = 0f;
                    }

                    await Task.Yield();
                }
            }
            catch (OperationCanceledException)
            {
                Debug.Log("UpdateLoop canceled.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"UpdateLoop exception: {ex.Message}");
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            var cross = other.GetComponent<Cross>();
            if (!cross || preCross == cross) return;

            preCross = cross;
            radius = other.GetComponent<CapsuleCollider>().radius;

            _ = HandleTriggerAsync(cross);
        }

        private async Task HandleTriggerAsync(Cross cross)
        {
            try
            {
                if (!reted)
                {
                    reted = true;
                    await nats.Send_CSoS_Async("ret", new Demand_CSoS { Id = id, Src = src, Dst = dst });
                }

                if (cross.number == goal)
                {
                    level++;
                    goal = RandomGoal();
                    Debug.Log($"[Goal] RM: {RMColor[id]} Goal count: {level} time[s]: {Time.time}");
                }
                
                // await ResourceDemandAsync();
                await semaphore.WaitAsync();
                try
                {
                    next = routeSearch.DirectionDijkstra(dst, goal);
                    if (next >= 0)
                    {
                        int isPermit = await nats.Send_CSoS_Async("next", new Demand_CSoS
                        {
                            Id = id,
                            Src = src,
                            Dst = dst,
                            Next = next,
                            Goal = goal,
                            Re = false
                        });

                        if (isPermit == 1)
                        {
                            Debug.Log($"[Route Set] RM: {RMColor[id]} Src: {src} Dst: {dst} Next: {next} Goal: {goal} time[s]: {Time.time}");
                            routeSearch.PrintFlags();
                            await SetNextAsync(next);
                            re = 0;
                        }
                        else
                        {
                            re = 1;
                        }
                    }
                    else
                    {
                        re = -next;
                    }
                }
                finally
                {
                    semaphore.Release();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"HandleTriggerAsync Exception: {ex.Message}");
            }
        }

        private void OnDestroy()
        {
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
        }

        private async Task ResourceDemandAsync()
        {
            try
            {
                var reply = await nats.Send_Resource_Async("resource", new Demand_Resource { Id = id });
                if (reply.ok)
                {
                    Debug.Log($"reply.ok: {reply.ok}");
                    await semaphore.WaitAsync();
                    try
                    {
                        Array.Copy(RebuildEdgeFlags(reply.edgeFlags, reply.edgeFlagRow), routeSearch.EdgeFlags, routeSearch.EdgeFlags.Length);
                        Array.Copy(reply.crossFlags.ToArray(), routeSearch.CrossFlags, routeSearch.CrossFlags.Length);
                        Debug.Log($"[Resource Update] RM: {RMColor[id]} time[s]: {Time.time}");
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"ResourceDemandAsync Exception: {ex.Message}");
            }
        }

        private async Task SetNextAsync(int next)
        {
            if (src == next)
            {
                handler.SetBack();
                await nats.Send_CSoS_Async("ret", new Demand_CSoS { Id = id, Src = src, Dst = dst });
            }
            else
            {
                reted = false;
                handler.SetCross(preCross.transform.position, preCross.GetDir(next));
            }

            src = preCross.number;
            dst = next;
        }

        private int RandomGoal()
        {
            var validIndices = Enumerable.Range(0, 11).Where(i => i != dst).ToArray();
            return validIndices[Random.Range(0, validIndices.Length)];
        }

        private bool[][] RebuildEdgeFlags(bool[] flatList, int rows)
        {
            int cols = flatList.Length / rows;
            if (flatList.Length % rows != 0)
                throw new ArgumentException("The flat list length is not evenly divisible by rows.");

            bool[][] result = new bool[rows][];
            for (int i = 0; i < rows; i++)
            {
                result[i] = new bool[cols];
                Array.Copy(flatList, i * cols, result[i], 0, cols);
            }
            return result;
        }
    }
}

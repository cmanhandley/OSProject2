using System;
using System.Collections.Generic;
using System.Linq;

namespace CPUSchedulingSimulator
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("CPU Scheduling Simulator");
            Console.WriteLine("=======================");

            // Prompt user for number of processes
            Console.Write("Enter number of processes: ");
            int numProcesses = int.Parse(Console.ReadLine());

            Console.Write("manual or automatic insertion (m/a): ");
            string insertionType = Console.ReadLine().ToLower();
            if (insertionType != "m" && insertionType != "a")
            {
                Console.WriteLine("Invalid input. Please enter 'm' for manual or 'a' for automatic.");
                throw new ArgumentException("Invalid input for insertion type.");
            }

            // Create list of processes
            List<Process> processes = new List<Process>();

            if (insertionType.Equals("m"))
            {
                // manual Input process details
                for (int i = 0; i < numProcesses; i++)
                {
                    Console.WriteLine($"\nProcess {i + 1}:");

                    Console.Write("Arrival Time: ");
                    int arrivalTime = int.Parse(Console.ReadLine());

                    Console.Write("Burst Time: ");
                    int burstTime = int.Parse(Console.ReadLine());

                    Console.Write("Priority (lower number means higher priority): ");
                    int priority = int.Parse(Console.ReadLine());

                    processes.Add(new Process(i + 1, arrivalTime, burstTime, priority));
                }
            }
            else {
                Random random = new Random();
                // automatic Input process details
                for (int i = 0; i < numProcesses; i++)
                {
                    Console.WriteLine($"\nProcess {i + 1}:");

                    Console.Write("Arrival Time: ");
                    int arrivalTime = random.Next(1, 100);

                    Console.Write("Burst Time: ");
                    int burstTime = random.Next(1, 100);

                    Console.Write("Priority (lower number means higher priority): ");
                    int priority = random.Next(1, 100);

                    processes.Add(new Process(i + 1, arrivalTime, burstTime, priority));
                }


            }


        

            // Initialize schedulers
            IScheduler fcfs = new FCFSScheduler();
            IScheduler sjf = new SJFScheduler();
            IScheduler srtf = new SRTFScheduler(); // One of the additional algorithms
            IScheduler hrrn = new HRRNScheduler(); // Second additional algorithm

            // Prompt for Round Robin quantum
            Console.Write("\nEnter time quantum for Round Robin: ");
            int quantum = int.Parse(Console.ReadLine());
            IScheduler rr = new RoundRobinScheduler(quantum);

            // Create a list of schedulers
            List<IScheduler> schedulers = new List<IScheduler> { fcfs, sjf, srtf, hrrn, rr };

            // Run simulations and display results
            Console.WriteLine("\nSimulation Results:");
            Console.WriteLine("==================");

            // Table header
            Console.WriteLine("{0,-25} {1,-15} {2,-15} {3,-15} {4,-15}",
                             "Scheduler", "Avg Wait Time", "Avg Turnaround", "CPU Util(%)", "Throughput");
            Console.WriteLine(new string('-', 85));

            // For each scheduler, run the simulation and print results
            foreach (var scheduler in schedulers)
            {
                // Create a deep copy of processes for each scheduler
                List<Process> processesCopy = DeepCopyProcesses(processes);

                // Run the scheduler
                SchedulerResult result = scheduler.Schedule(processesCopy);

                // Display results
                Console.WriteLine("{0,-25} {1,-15:F2} {2,-15:F2} {3,-15:F2} {4,-15:F2}",
                                 scheduler.Name,
                                 result.AverageWaitingTime,
                                 result.AverageTurnaroundTime,
                                 result.CPUUtilization,
                                 result.Throughput);

                // Display Gantt chart
                Console.WriteLine("\nGantt Chart for " + scheduler.Name + ":");
                Console.WriteLine(result.GanttChart);
                Console.WriteLine();
            }

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }

        // Helper method to create a deep copy of processes
        static List<Process> DeepCopyProcesses(List<Process> original)
        {
            List<Process> copy = new List<Process>();
            foreach (var process in original)
            {
                copy.Add(new Process(
                    process.Id,
                    process.ArrivalTime,
                    process.BurstTime,
                    process.Priority
                ));
            }
            return copy;
        }
    }

    // Process class representing a process in the system
    class Process
    {
        public int Id { get; set; }
        public int ArrivalTime { get; set; }
        public int BurstTime { get; set; }
        public int RemainingTime { get; set; }
        public int Priority { get; set; }
        public int WaitingTime { get; set; }
        public int TurnaroundTime { get; set; }
        public int CompletionTime { get; set; }
        public int ResponseTime { get; set; }
        public bool Started { get; set; }

        public Process(int id, int arrivalTime, int burstTime, int priority)
        {
            Id = id;
            ArrivalTime = arrivalTime;
            BurstTime = burstTime;
            RemainingTime = burstTime;
            Priority = priority;
            WaitingTime = 0;
            TurnaroundTime = 0;
            CompletionTime = 0;
            ResponseTime = -1; // -1 indicates not started yet
            Started = false;
        }
    }

    // Class to hold simulation results
    class SchedulerResult
    {
        public double AverageWaitingTime { get; set; }
        public double AverageTurnaroundTime { get; set; }
        public double CPUUtilization { get; set; }
        public double Throughput { get; set; }
        public string GanttChart { get; set; }
    }

    // Interface for scheduler algorithms
    interface IScheduler
    {
        string Name { get; }
        SchedulerResult Schedule(List<Process> processes);
    }

    // First Come First Served Scheduler
    class FCFSScheduler : IScheduler
    {
        public string Name => "First Come First Served";

        public SchedulerResult Schedule(List<Process> processes)
        {
            // Sort processes by arrival time
            processes = processes.OrderBy(p => p.ArrivalTime).ToList();

            int currentTime = 0;
            int totalBurstTime = 0;
            string ganttChart = "";

            foreach (var process in processes)
            {
                // If the process arrives after the current time, there's idle time
                if (process.ArrivalTime > currentTime)
                {
                    ganttChart += $"| Idle ({currentTime}-{process.ArrivalTime}) ";
                    currentTime = process.ArrivalTime;
                }

                // Set response time if this is the first time the process is being executed
                if (!process.Started)
                {
                    process.ResponseTime = currentTime - process.ArrivalTime;
                    process.Started = true;
                }

                // Process the current process
                ganttChart += $"| P{process.Id} ({currentTime}-{currentTime + process.BurstTime}) ";
                currentTime += process.BurstTime;
                totalBurstTime += process.BurstTime;

                // Set completion time and calculate turnaround and waiting time
                process.CompletionTime = currentTime;
                process.TurnaroundTime = process.CompletionTime - process.ArrivalTime;
                process.WaitingTime = process.TurnaroundTime - process.BurstTime;
            }

            ganttChart += "|";

            // Calculate average waiting and turnaround times
            double avgWaitingTime = processes.Average(p => p.WaitingTime);
            double avgTurnaroundTime = processes.Average(p => p.TurnaroundTime);

            // Calculate CPU utilization and throughput
            int totalTime = currentTime;
            double cpuUtilization = (double)totalBurstTime / totalTime * 100;
            double throughput = (double)processes.Count / totalTime;

            return new SchedulerResult
            {
                AverageWaitingTime = avgWaitingTime,
                AverageTurnaroundTime = avgTurnaroundTime,
                CPUUtilization = cpuUtilization,
                Throughput = throughput,
                GanttChart = ganttChart
            };
        }
    }

    // Shortest Job First Scheduler (Non-preemptive)
    class SJFScheduler : IScheduler
    {
        public string Name => "Shortest Job First";

        public SchedulerResult Schedule(List<Process> processes)
        {
            // Sort processes by arrival time initially
            processes = processes.OrderBy(p => p.ArrivalTime).ToList();

            int currentTime = 0;
            int totalBurstTime = 0;
            string ganttChart = "";

            // Keep track of completed processes
            int completedProcesses = 0;

            while (completedProcesses < processes.Count)
            {
                // Find the process with the shortest burst time that has arrived
                Process nextProcess = null;
                int minBurstTime = int.MaxValue;

                // Check all processes that have arrived by the current time
                foreach (var process in processes)
                {
                    if (process.ArrivalTime <= currentTime && process.RemainingTime > 0 && process.BurstTime < minBurstTime)
                    {
                        minBurstTime = process.BurstTime;
                        nextProcess = process;
                    }
                }

                // If no process is available, advance time to the next arrival
                if (nextProcess == null)
                {
                    // Find the next arriving process
                    Process nextArriving = processes.Where(p => p.ArrivalTime > currentTime && p.RemainingTime > 0)
                                                  .OrderBy(p => p.ArrivalTime)
                                                  .FirstOrDefault();

                    if (nextArriving != null)
                    {
                        ganttChart += $"| Idle ({currentTime}-{nextArriving.ArrivalTime}) ";
                        currentTime = nextArriving.ArrivalTime;
                    }
                    else
                    {
                        // Should not happen if the input is valid
                        break;
                    }
                }
                else
                {
                    // Set response time if this is the first time the process is being executed
                    if (!nextProcess.Started)
                    {
                        nextProcess.ResponseTime = currentTime - nextProcess.ArrivalTime;
                        nextProcess.Started = true;
                    }

                    // Process the selected process
                    ganttChart += $"| P{nextProcess.Id} ({currentTime}-{currentTime + nextProcess.BurstTime}) ";
                    currentTime += nextProcess.BurstTime;
                    totalBurstTime += nextProcess.BurstTime;

                    // Set completion time and calculate turnaround and waiting time
                    nextProcess.CompletionTime = currentTime;
                    nextProcess.TurnaroundTime = nextProcess.CompletionTime - nextProcess.ArrivalTime;
                    nextProcess.WaitingTime = nextProcess.TurnaroundTime - nextProcess.BurstTime;
                    nextProcess.RemainingTime = 0;

                    completedProcesses++;
                }
            }

            ganttChart += "|";

            // Calculate average waiting and turnaround times
            double avgWaitingTime = processes.Average(p => p.WaitingTime);
            double avgTurnaroundTime = processes.Average(p => p.TurnaroundTime);

            // Calculate CPU utilization and throughput
            int totalTime = currentTime;
            double cpuUtilization = (double)totalBurstTime / totalTime * 100;
            double throughput = (double)processes.Count / totalTime;

            return new SchedulerResult
            {
                AverageWaitingTime = avgWaitingTime,
                AverageTurnaroundTime = avgTurnaroundTime,
                CPUUtilization = cpuUtilization,
                Throughput = throughput,
                GanttChart = ganttChart
            };
        }
    }

    // Shortest Remaining Time First Scheduler (Preemptive)
    class SRTFScheduler : IScheduler
    {
        public string Name => "Shortest Remaining Time First";

        public SchedulerResult Schedule(List<Process> processes)
        {
            // Create a list of processes sorted by arrival time
            processes = processes.OrderBy(p => p.ArrivalTime).ToList();

            int currentTime = 0;
            int completedProcesses = 0;
            int totalBurstTime = processes.Sum(p => p.BurstTime);
            string ganttChart = "";

            Process currentProcess = null;
            int lastProcessId = -1;

            // Continue until all processes are completed
            while (completedProcesses < processes.Count)
            {
                // Find process with shortest remaining time among arrived processes
                Process shortestProcess = null;
                int shortestRemainingTime = int.MaxValue;

                foreach (var process in processes)
                {
                    if (process.ArrivalTime <= currentTime && process.RemainingTime > 0 &&
                        process.RemainingTime < shortestRemainingTime)
                    {
                        shortestRemainingTime = process.RemainingTime;
                        shortestProcess = process;
                    }
                }

                // If no process is available, advance time to the next arrival
                if (shortestProcess == null)
                {
                    Process nextProcess = processes.Where(p => p.ArrivalTime > currentTime && p.RemainingTime > 0)
                                                 .OrderBy(p => p.ArrivalTime)
                                                 .FirstOrDefault();

                    if (nextProcess != null)
                    {
                        ganttChart += $"| Idle ({currentTime}-{nextProcess.ArrivalTime}) ";
                        currentTime = nextProcess.ArrivalTime;
                    }
                    else
                    {
                        // Should not happen if the input is valid
                        break;
                    }
                }
                else
                {
                    // Set response time if this is the first time the process is being executed
                    if (!shortestProcess.Started)
                    {
                        shortestProcess.ResponseTime = currentTime - shortestProcess.ArrivalTime;
                        shortestProcess.Started = true;
                    }

                    // Check if we need to add a new entry to the Gantt chart
                    if (lastProcessId != shortestProcess.Id)
                    {
                        ganttChart += $"| P{shortestProcess.Id} ({currentTime}-";
                        lastProcessId = shortestProcess.Id;
                    }

                    // Execute the process for 1 time unit
                    currentTime++;
                    shortestProcess.RemainingTime--;

                    // Check if the current process is completed
                    if (shortestProcess.RemainingTime == 0)
                    {
                        completedProcesses++;
                        shortestProcess.CompletionTime = currentTime;
                        shortestProcess.TurnaroundTime = shortestProcess.CompletionTime - shortestProcess.ArrivalTime;
                        shortestProcess.WaitingTime = shortestProcess.TurnaroundTime - shortestProcess.BurstTime;

                        // Update Gantt chart
                        ganttChart += $"{currentTime}) ";
                        lastProcessId = -1;
                    }
                    // Check if a new process arrives with shorter remaining time
                    else
                    {
                        bool preempt = false;
                        foreach (var process in processes)
                        {
                            if (process.ArrivalTime == currentTime && process.RemainingTime < shortestProcess.RemainingTime)
                            {
                                preempt = true;
                                break;
                            }
                        }

                        if (preempt)
                        {
                            ganttChart += $"{currentTime}) ";
                            lastProcessId = -1;
                        }
                    }
                }
            }

            ganttChart += "|";

            // Calculate average waiting and turnaround times
            double avgWaitingTime = processes.Average(p => p.WaitingTime);
            double avgTurnaroundTime = processes.Average(p => p.TurnaroundTime);

            // Calculate CPU utilization and throughput
            int totalTime = currentTime;
            double cpuUtilization = (double)totalBurstTime / totalTime * 100;
            double throughput = (double)processes.Count / totalTime;

            return new SchedulerResult
            {
                AverageWaitingTime = avgWaitingTime,
                AverageTurnaroundTime = avgTurnaroundTime,
                CPUUtilization = cpuUtilization,
                Throughput = throughput,
                GanttChart = ganttChart
            };
        }
    }

    // Highest Response Ratio Next Scheduler
    class HRRNScheduler : IScheduler
    {
        public string Name => "Highest Response Ratio Next";

        public SchedulerResult Schedule(List<Process> processes)
        {
            // Sort processes by arrival time initially
            processes = processes.OrderBy(p => p.ArrivalTime).ToList();

            int currentTime = 0;
            int totalBurstTime = 0;
            string ganttChart = "";

            // Keep track of completed processes
            int completedProcesses = 0;

            while (completedProcesses < processes.Count)
            {
                // Find the process with the highest response ratio
                Process nextProcess = null;
                double highestResponseRatio = -1;

                foreach (var process in processes)
                {
                    if (process.ArrivalTime <= currentTime && process.RemainingTime > 0)
                    {
                        double waitingTime = currentTime - process.ArrivalTime;
                        double responseRatio = (waitingTime + process.BurstTime) / process.BurstTime;

                        if (responseRatio > highestResponseRatio)
                        {
                            highestResponseRatio = responseRatio;
                            nextProcess = process;
                        }
                    }
                }

                // If no process is available, advance time to the next arrival
                if (nextProcess == null)
                {
                    Process nextArriving = processes.Where(p => p.ArrivalTime > currentTime && p.RemainingTime > 0)
                                                  .OrderBy(p => p.ArrivalTime)
                                                  .FirstOrDefault();

                    if (nextArriving != null)
                    {
                        ganttChart += $"| Idle ({currentTime}-{nextArriving.ArrivalTime}) ";
                        currentTime = nextArriving.ArrivalTime;
                    }
                    else
                    {
                        // Should not happen if the input is valid
                        break;
                    }
                }
                else
                {
                    // Set response time if this is the first time the process is being executed
                    if (!nextProcess.Started)
                    {
                        nextProcess.ResponseTime = currentTime - nextProcess.ArrivalTime;
                        nextProcess.Started = true;
                    }

                    // Process the selected process
                    ganttChart += $"| P{nextProcess.Id} ({currentTime}-{currentTime + nextProcess.BurstTime}) ";
                    currentTime += nextProcess.BurstTime;
                    totalBurstTime += nextProcess.BurstTime;

                    // Set completion time and calculate turnaround and waiting time
                    nextProcess.CompletionTime = currentTime;
                    nextProcess.TurnaroundTime = nextProcess.CompletionTime - nextProcess.ArrivalTime;
                    nextProcess.WaitingTime = nextProcess.TurnaroundTime - nextProcess.BurstTime;
                    nextProcess.RemainingTime = 0;

                    completedProcesses++;
                }
            }

            ganttChart += "|";

            // Calculate average waiting and turnaround times
            double avgWaitingTime = processes.Average(p => p.WaitingTime);
            double avgTurnaroundTime = processes.Average(p => p.TurnaroundTime);

            // Calculate CPU utilization and throughput
            int totalTime = currentTime;
            double cpuUtilization = (double)totalBurstTime / totalTime * 100;
            double throughput = (double)processes.Count / totalTime;

            return new SchedulerResult
            {
                AverageWaitingTime = avgWaitingTime,
                AverageTurnaroundTime = avgTurnaroundTime,
                CPUUtilization = cpuUtilization,
                Throughput = throughput,
                GanttChart = ganttChart
            };
        }
    }

    // Round Robin Scheduler
    class RoundRobinScheduler : IScheduler
    {
        private int quantum;

        public string Name => $"Round Robin (q={quantum})";

        public RoundRobinScheduler(int quantum)
        {
            this.quantum = quantum;
        }

        public SchedulerResult Schedule(List<Process> processes)
        {
            // Sort processes by arrival time initially
            processes = processes.OrderBy(p => p.ArrivalTime).ToList();

            Queue<Process> readyQueue = new Queue<Process>();
            int currentTime = 0;
            int totalBurstTime = processes.Sum(p => p.BurstTime);
            string ganttChart = "";

            // Keep track of completed processes
            int completedProcesses = 0;

            // Add first process to ready queue
            int i = 0;
            if (processes.Count > 0)
            {
                // If the first process doesn't arrive at time 0, add idle time
                if (processes[0].ArrivalTime > 0)
                {
                    ganttChart += $"| Idle (0-{processes[0].ArrivalTime}) ";
                    currentTime = processes[0].ArrivalTime;
                }

                readyQueue.Enqueue(processes[0]);
                i = 1;
            }

            while (completedProcesses < processes.Count)
            {
                // If the ready queue is empty, move time forward to the next arrival
                if (readyQueue.Count == 0)
                {
                    if (i < processes.Count)
                    {
                        ganttChart += $"| Idle ({currentTime}-{processes[i].ArrivalTime}) ";
                        currentTime = processes[i].ArrivalTime;
                        readyQueue.Enqueue(processes[i]);
                        i++;
                    }
                    else
                    {
                        // All processes have arrived and been processed at least once
                        break;
                    }
                }
                else
                {
                    // Get the next process from ready queue
                    Process currentProcess = readyQueue.Dequeue();

                    // Set response time if this is the first time the process is being executed
                    if (!currentProcess.Started)
                    {
                        currentProcess.ResponseTime = currentTime - currentProcess.ArrivalTime;
                        currentProcess.Started = true;
                    }

                    // Calculate execution time for this quantum
                    int executionTime = Math.Min(quantum, currentProcess.RemainingTime);

                    // Execute the process for the quantum or until completion
                    ganttChart += $"| P{currentProcess.Id} ({currentTime}-{currentTime + executionTime}) ";
                    currentTime += executionTime;
                    currentProcess.RemainingTime -= executionTime;

                    // Check for new arrivals during this time quantum
                    while (i < processes.Count && processes[i].ArrivalTime <= currentTime)
                    {
                        readyQueue.Enqueue(processes[i]);
                        i++;
                    }

                    // Check if the process has completed
                    if (currentProcess.RemainingTime == 0)
                    {
                        completedProcesses++;
                        currentProcess.CompletionTime = currentTime;
                        currentProcess.TurnaroundTime = currentProcess.CompletionTime - currentProcess.ArrivalTime;
                        currentProcess.WaitingTime = currentProcess.TurnaroundTime - currentProcess.BurstTime;
                    }
                    else
                    {
                        // If not completed, add back to ready queue
                        readyQueue.Enqueue(currentProcess);
                    }
                }
            }

            ganttChart += "|";

            // Calculate average waiting and turnaround times
            double avgWaitingTime = processes.Average(p => p.WaitingTime);
            double avgTurnaroundTime = processes.Average(p => p.TurnaroundTime);

            // Calculate CPU utilization and throughput
            int totalTime = currentTime;
            double cpuUtilization = (double)totalBurstTime / totalTime * 100;
            double throughput = (double)processes.Count / totalTime;

            return new SchedulerResult
            {
                AverageWaitingTime = avgWaitingTime,
                AverageTurnaroundTime = avgTurnaroundTime,
                CPUUtilization = cpuUtilization,
                Throughput = throughput,
                GanttChart = ganttChart
            };
        }
    }
}
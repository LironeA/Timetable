using Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public class GeneticAlgorithm
    {
        public int PopulationSize { get; set; }
        private readonly double mutationRate;

        public List<WeekTimeTable> Population { get; private set; }
        private List<Group> Groups;
        private List<Subject> Subjects;
        private List<Teacher> Teachers;
        private List<Room> Rooms;
        public List<GroupProgram> GroupPrograms { get; set; }

        public GeneticAlgorithm(int populationSize, Data data, double mutationRate)
        {
            PopulationSize = populationSize;
            Groups = data.Groups;
            Subjects = data.Subjects;
            Teachers = data.Teachers;
            Rooms = data.Rooms;
            GroupPrograms = data.GroupPrograms;
            Population = GenerateInitialPopulation();
            this.mutationRate = mutationRate;
        }

        // Генерація початкової популяції
        private List<WeekTimeTable> GenerateInitialPopulation()
        {
            var initialPopulation = new List<WeekTimeTable>();
            var random = new Random();

            for(int i = 0; i < PopulationSize; i++)
            {
                var weekTimeTable = new WeekTimeTable(Groups);

                foreach(var groupTimeTable in weekTimeTable.GroupTimeTables)
                {
                    foreach(var dayTimeTable in groupTimeTable.DayTimeTables)
                    {
                        for(int lessonIndex = 0; lessonIndex < dayTimeTable.TimeTableCells.Length; lessonIndex++)
                        {
                            var cell = new TimeTableCell();

                            var groupProgramm = GroupPrograms.FirstOrDefault(x => x.GroupId == groupTimeTable.Group.Id);
                            // Випадковий вибір предмета
                            var subject = groupProgramm.SubjectIds[random.Next(groupProgramm.SubjectIds.Count)];
                            cell.Subject = Subjects.FirstOrDefault(x => x.Id == subject);

                            // Випадковий вибір викладача, який може викладати обраний предмет
                            var suitableTeachers = Teachers.Where(t => t.SubjectIds.Contains(cell.Subject.Id)).ToList();
                            if(suitableTeachers.Any())
                            {
                                cell.Teacher = suitableTeachers[random.Next(suitableTeachers.Count)];
                            }

                            // Вибір аудиторії, яка підходить по місткості
                            cell.Room = Rooms[random.Next(Rooms.Count)];

                            // Випадковий тип заняття (лекція або практичне)
                            cell.IsLecture = random.Next(2) == 0;

                            dayTimeTable.TimeTableCells[lessonIndex] = cell;
                        }
                    }
                }

                // Обчислюємо фітнес для кожного індивіда в популяції
                weekTimeTable.CalculateFitness();
                initialPopulation.Add(weekTimeTable);
            }

            return initialPopulation;
        }


        public void Run(int N)
        {
            for(int i = 0; i < N; i++)
            {
                Step();


                //if(i % 1000 == 0)
                {
                    Console.WriteLine($"Step {i + 1}: Best fitness = {Population.Max(t => t.Fittness)}");
                }
            }
        }

        public void Step()
        {
            Population = Population.OrderByDescending(t => t.Fittness).Take(PopulationSize / 2).ToList();

            // Кросовер
            var offspring = Crossover(Population);

            // Мутація
            Mutate(offspring);

            // Оновлення популяції: заміна найгірших особин новими
            Population = Population.Concat(offspring).ToList();

            Parallel.ForEach(Population, timetable => timetable.CalculateFitness());
        }

        private List<WeekTimeTable> Crossover(List<WeekTimeTable> partents)
        {
            var rand = new Random();
            var res = new List<WeekTimeTable>();
            for(int i = 0; i < partents.Count; i += 2)
            {
                var p1 = partents[i];
                var p2 = partents[i + 1];
                int randGroupIndex = rand.Next(p1.GroupTimeTables.Length);
                var child1 = new WeekTimeTable(Groups);
                var child2 = new WeekTimeTable(Groups);
                for(int j = 0; j < p1.GroupTimeTables.Length; j++)
                {
                    child1.GroupTimeTables[j] = j < randGroupIndex ? p1.GroupTimeTables[j].Copy() : p2.GroupTimeTables[j].Copy();
                    child2.GroupTimeTables[j] = j < randGroupIndex ? p2.GroupTimeTables[j].Copy() : p1.GroupTimeTables[j].Copy();
                }
                res.Add(child1);
                res.Add(child2);
            }
            return res;
        }

        private void Mutate(List<WeekTimeTable> offspring)
        {
            var random = new Random();
            foreach(var item in offspring)
            {
                if(random.NextDouble() < mutationRate)
                {
                    var randGene = item.GroupTimeTables[random.Next(item.GroupTimeTables.Length)].DayTimeTables[random.Next(5)].TimeTableCells[random.Next(4)];
                    randGene.Subject = Subjects[random.Next(Subjects.Count)];
                    randGene.Teacher = Teachers.FirstOrDefault(t => t.SubjectIds.Contains(randGene.Subject.Id));
                    randGene.Room = Rooms[random.Next(Rooms.Count)];
                    randGene.IsLecture = random.Next(2) == 0;

                }
            }
        }

        private void UpdatePopulation(List<WeekTimeTable> offspring)
        {
            // Оновлення популяції, заміна найгірших особин новими
            Population = Population.OrderByDescending(t => t.Fittness).Take(PopulationSize - offspring.Count).Concat(offspring).ToList();
        }
    }
}

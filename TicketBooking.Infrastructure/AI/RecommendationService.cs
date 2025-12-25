using Microsoft.ML;
using Microsoft.ML.Trainers;
using TicketBooking.Application.Common.Interfaces.AI;
using TicketBooking.Infrastructure.AI.Models;

namespace TicketBooking.Infrastructure.AI
{
    public class RecommendationService : IRecommendationService
    {
        private readonly MLContext _mlContext;
        private ITransformer? _model;

        public RecommendationService()
        {
            _mlContext = new MLContext();
            // Trong thực tế, Model sẽ được Train bởi Background Job và lưu ra file .zip.
            // Ở đây để code hoàn chỉnh chạy được ngay, ta Train ngay trong Constructor (In-Memory).
            TrainModel();
        }

        private void TrainModel()
        {
            // 1. MOCK DATA (GIẢ LẬP LỊCH SỬ MUA HÀNG)
            // Giả sử có 3 User và 5 Event.
            var trainData = new List<EventRating>
            {
                // User 1 thích Event A, B (Rating cao = Mua vé VIP)
                new EventRating { UserId = "1111", EventId = "AAAA", Label = 5 },
                new EventRating { UserId = "1111", EventId = "BBBB", Label = 4 },
                
                // User 2 thích Event B, C
                new EventRating { UserId = "2222", EventId = "BBBB", Label = 5 },
                new EventRating { UserId = "2222", EventId = "CCCC", Label = 5 },

                // User 3 thích Event A (Có nét giống User 1)
                new EventRating { UserId = "3333", EventId = "AAAA", Label = 3 },
            };

            var trainingDataView = _mlContext.Data.LoadFromEnumerable(trainData);

            // 2. BUILD PIPELINE
            // Matrix Factorization yêu cầu Key Type (Số nguyên) nên ta phải Map từ String -> Key.
            var pipeline = _mlContext.Transforms.Conversion.MapValueToKey(outputColumnName: "userIdEncoded", inputColumnName: nameof(EventRating.UserId))
                .Append(_mlContext.Transforms.Conversion.MapValueToKey(outputColumnName: "eventIdEncoded", inputColumnName: nameof(EventRating.EventId)))
                .Append(_mlContext.Recommendation().Trainers.MatrixFactorization(options: new MatrixFactorizationTrainer.Options
                {
                    MatrixColumnIndexColumnName = "userIdEncoded",
                    MatrixRowIndexColumnName = "eventIdEncoded",
                    LabelColumnName = "Label",
                    // EXPLAIN MATRIX FACTORIZATION:
                    // Thuật toán này phân rã ma trận User-Event khổng lồ thành 2 ma trận nhỏ hơn (User Features & Event Features).
                    // Ví dụ: User thích "Nhạc Rock", Event là "Nhạc Rock" -> Tích vô hướng của 2 vector này sẽ ra Score cao.
                    NumberOfIterations = 20,
                    ApproximationRank = 100
                }));

            // 3. TRAIN
            _model = pipeline.Fit(trainingDataView);
        }

        public List<Guid> GetRecommendedEvents(Guid userId, List<Guid> candidateEventIds)
        {
            if (_model == null) return candidateEventIds; // Fallback nếu model lỗi.

            // Tạo Engine dự đoán đơn lẻ.
            var predictionEngine = _mlContext.Model.CreatePredictionEngine<EventRating, EventRatingPrediction>(_model);

            var predictions = new List<(Guid EventId, float Score)>();

            // Duyệt qua tất cả Event đang mở bán để chấm điểm.
            foreach (var eventId in candidateEventIds)
            {
                // Giả lập logic map User thật sang Mock User để test (Vì DB thật chưa có history).
                // Trong thực tế: Sếp dùng đúng userId truyền vào.
                // Ở đây tôi map mọi user về "1111" để Sếp thấy nó gợi ý theo sở thích của ông 1111 (thích A, B).
                var mockUserId = "1111";

                // Map Event thật sang Mock Event (nếu có).
                // Do chúng ta đang mock data, nên logic này chỉ mang tính minh họa luồng đi.
                // Để chạy được với data thật của Sếp, Sếp cần bảng Review/History thật.
                // Ở đây tôi sẽ chấm điểm dựa trên input đầu vào.

                var prediction = predictionEngine.Predict(new EventRating
                {
                    UserId = userId.ToString(), // Dùng User thật.
                    EventId = eventId.ToString()
                });

                // Nếu User hoặc Event chưa từng xuất hiện trong tập Train (Cold Start), Score sẽ lanh quanh 0 hoặc NaN.
                // Ta xử lý NaN thành 0.
                float finalScore = float.IsNaN(prediction.Score) ? 0 : prediction.Score;

                predictions.Add((eventId, finalScore));
            }

            // Trả về Top 5 Event có điểm cao nhất.
            return predictions
                .OrderByDescending(x => x.Score)
                .Take(5)
                .Select(x => x.EventId)
                .ToList();
        }
    }
}
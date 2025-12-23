# =======================================================
# STAGE 1: BUILD (Sử dụng SDK Image để biên dịch code)
# Image này chứa đầy đủ công cụ: dotnet CLI, NuGet, MSBuild...
# =======================================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# --- BƯỚC 1: COPY FILE CẤU HÌNH DỰ ÁN (.CSPROJ) ---
# Tại sao copy riêng từng cái? Để tận dụng Docker Cache.
# Nếu Sếp chỉ sửa code logic (.cs) mà không cài thêm thư viện mới,
# Docker sẽ bỏ qua bước Restore này (vì file .csproj không đổi) -> Tiết kiệm 3-5 phút build.

# Copy file csproj của từng tầng theo đúng cấu trúc thư mục
COPY ["TicketBooking.API/TicketBooking.API.csproj", "TicketBooking.API/"]
COPY ["TicketBooking.Application/TicketBooking.Application.csproj", "TicketBooking.Application/"]
COPY ["TicketBooking.Domain/TicketBooking.Domain.csproj", "TicketBooking.Domain/"]
COPY ["TicketBooking.Infrastructure/TicketBooking.Infrastructure.csproj", "TicketBooking.Infrastructure/"]

# --- BƯỚC 2: RESTORE DEPENDENCIES ---
# Tải tất cả các gói Nuget về. Vì API tham chiếu đến 3 thằng kia,
# nên lệnh này sẽ restore cho cả Solution thông qua API project.
RUN dotnet restore "TicketBooking.API/TicketBooking.API.csproj"

# --- BƯỚC 3: COPY TOÀN BỘ SOURCE CODE ---
# Sau khi restore xong xuôi mới copy code vào.
COPY . .

# --- BƯỚC 4: BUILD (BIÊN DỊCH) ---
# Chuyển vào thư mục API để làm việc
WORKDIR "/src/TicketBooking.API"
# Build bản Release (Tối ưu hiệu năng)
RUN dotnet build "TicketBooking.API.csproj" -c Release -o /app/build

# --- BƯỚC 5: PUBLISH (ĐÓNG GÓI) ---
# Gom tất cả DLL, Config, Static files vào một thư mục gọn gàng
FROM build AS publish
RUN dotnet publish "TicketBooking.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# =======================================================
# STAGE 2: RUNTIME (Sử dụng Runtime Image để chạy)
# Image này siêu nhẹ (chỉ ~200MB), KHÔNG chứa SDK.
# Hacker vào đây cũng không thể biên dịch mã độc được.
# =======================================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Cấu hình User non-root (Bảo mật hơn root mặc định)
USER app

# Mở cổng 8080 (Chuẩn mới của .NET 8 Container)
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080

# Copy kết quả từ Stage 1 (thư mục /app/publish) sang Stage 2
COPY --from=publish /app/publish .

# Lệnh chạy ứng dụng khi container khởi động
ENTRYPOINT ["dotnet", "TicketBooking.API.dll"]
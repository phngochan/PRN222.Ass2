const connection = new signalR.HubConnectionBuilder()
    .withUrl("/vehicleHub")
    .build();

connection.on("VehicleCreated", function (id, model) {
    // Hiển thị thông báo hoặc cập nhật UI khi có xe mới
    alert(`Xe mới được tạo: ${model} (ID: ${id})`);
});

connection.on("VehicleUpdated", function (id, model) {
    // Hiển thị thông báo hoặc cập nhật UI khi xe được cập nhật
    alert(`Xe đã được cập nhật: ${model} (ID: ${id})`);
});

connection.on("VehicleDeleted", function (id) {
    // Hiển thị thông báo hoặc cập nhật UI khi xe bị xóa
    alert(`Xe đã bị xóa (ID: ${id})`);
});

connection.start().catch(function (err) {
    return console.error(err.toString());
});

# EasyIotSharp

#### 介绍
✨ 核心特性
🛰 多协议支持（MQTT/HTTP/CoAP）

📊 实时数据可视化

🎮 3D场景交互控制

⚡ 毫秒级设备响应

🔗 场景联动引擎

🔒 双向TLS加密通信

#### 软件架构
软件架构说明

#### 使用说明

📡 MQTT 通讯主题规范文档

📌 设备通信协议 (DeviceMQTT)

####  设备数据上报主题  ####
📤 设备上报类主题
# 主题格式                           #用途说明
| -------------------------- | --------------------- |
| `{项目id}/{设备id}/rawdata`    | 设备原始数据上报（传感器数据、采集数据等） |
# 低频数据示例
{
  "TenantAbbreviation": "ABC", // 租户简称，用于标识数据所属的租户
  "ProjectId": "P2024001", // 项目ID，标识数据所属的项目
  "Time": "2024-05-15T10:30:00", // 时间戳，表示数据采集的时间点
  "PointType": "Temperature", // 测点类型，如温度、湿度、压力等
  "DataType": 1, // 数据类型，1表示低频数据
  "Points": [ // 测点数据列表，包含多个测点的数据
    {
      "PointId": "TP001", // 测点ID，唯一标识一个测点
      "IndicatorCount": 2, // 指标数量，表示该测点包含的指标个数
      "Values": [ // 数据值列表，包含测点的各项指标值
        {
          "Name": "Temperature", // 指标名称，如温度
          "Value": 25.6, // 指标值，数值类型
          "Unit": "°C" // 单位，如摄氏度
        },
        {
          "Name": "Humidity", // 指标名称，如湿度
          "Value": 65.3, // 指标值，数值类型
          "Unit": "%" // 单位，如百分比
        }
      ]
    }
  ]
}

# 高频数据示例
{
  "TenantAbbreviation": "XYZ", // 租户简称
  "ProjectId": "P2024002", // 项目ID
  "Time": "2024-05-15T14:45:00", // 时间戳
  "PointType": "Vibration", // 测点类型，如振动
  "DataType": 2, // 数据类型，2表示高频数据
  "SamplingPeriod": 10, // 采集周期，单位为毫秒(ms)，表示每10ms采集一次数据
  "Points": [ // 高频测点数据列表
    {
      "PointId": "VP001", // 测点ID
      "IndicatorCount": 3, // 指标数量，表示该测点包含3个指标
      "SampleGroups": [ // 采样数据组，每组包含多个指标的采样值
        [0.01, 0.02, 0.03], // 第1次采样的3个指标值
        [0.02, 0.03, 0.04], // 第2次采样的3个指标值
        [0.03, 0.04, 0.05], // 第3次采样的3个指标值
        [0.04, 0.05, 0.06], // 第4次采样的3个指标值
        [0.05, 0.06, 0.07]  // 第5次采样的3个指标值
      ]
    } 
  ]
}
| `{项目id}/{设备id}/status`     | 设备状态上报（在线/离线、电量等）     |
{
  "timestamp": 1714348795,
  "deviceId": "CX-E-1",
  "projectId": "project001",
  "status": {
    "online": true,
    "battery": 85,
    "signal": 4,
    "workMode": "normal",
    "errorCode": 0
  },
  "lastActiveTime": "2024-05-28T10:13:15.000Z",
  "ipAddress": "192.168.1.100",
  "firmwareVersion": "1.2.3"
}
| `{项目id}/{设备id}/properties` | 设备属性变更上报（如配置参数变化）     |
{
  "timestamp": 1714348795,
  "deviceId": "CX-E-1",
  "projectId": "project001",
  "properties": {
    "sampleRate": 5,
    "alarmThreshold": 80,
    "displayMode": "graph",
    "powerSaveMode": true
  },
  "changeReason": "user",
  "operator": "admin"
}

| `{项目id}/{设备id}/events`     | 设备事件上报（异常、告警等）        |
{
  "timestamp": 1714348795,
  "deviceId": "CX-E-1",
  "projectId": "project001",
  "eventType": "alarm",
  "eventLevel": "critical",
  "eventId": "evt_12345",
  "message": "温度过高",
  "details": {
    "value": 85.6,
    "threshold": 80,
    "duration": 300
  },
  "status": "active"
}


####  设备控制下发主题  ####
📥 控制类主题
# 主题格式                           #用途说明
| ---------------------- | ------------------ |
| `{项目id}/{设备id}/cmd`    | 控制命令下发（如开关控制）      |
| `{项目id}/{设备id}/config` | 配置信息下发（如参数设置）      |
| `{项目id}/{设备id}/ota`    | 固件升级包下发（远程 OTA 升级） |

####  设备响应主题  ####
🔄 响应类主题
# 主题格式                           #用途说明     |
| ------------------------------- | ---------- |
| `{项目id}/{设备id}/cmd/response`    | 设备对控制命令的响应 |
| `{项目id}/{设备id}/config/response` | 设备对配置命令的响应 |

####  设备管理类主题  ####
🕹 设备管理类主题
# 主题格式                           #用途说明     |
| ---------------------------- | ------------- |
| `{项目id}/{设备id}/register`     | 设备注册（首次上线）    |
| `{项目id}/{设备id}/heartbeat`    | 心跳包上报（保持连接状态） |
| `{项目id}/system/notification` | 系统广播通知（如维护通知） |

####  分组与场景联动主题  ####
🎭 场景联动主题
# 主题格式                           #用途说明     |
| ----------------------------- | -------------- |
| `{项目id}/group/{分组id}/cmd`     | 对分组设备下发命令      |
| `{项目id}/scene/{场景id}/trigger` | 触发场景联动（如自动化执行） |

🎮 Unity交互协议 (UnityMQTT)
####  分组与场景联动主题  ####
# 主题格式                           #用途说明     |
| --------------------------------------------------- | ---------------- |
| `iot/project/{projectId}/device/{deviceId}/status`  | Unity上报设备状态      | 
{
  "deviceId": "CX-E-1",
  "status": {
    "temperature": 85.6,
    "humidity": 15
  }
}
| `iot/project/{projectId}/device/{deviceId}/control` | 平台向Unity下发设备控制命令 |
{
  "deviceId": "CX-E-1",
  "controlType": "visual",
  "params": {
    "color": "#FF0000",
    "visible": true
  }
}
####  UI 点位交互事件  ####
# 主题格式                           #用途说明     |
| --------------------------------------------- | -------- |
| `iot/project/{projectId}/ui/model/click`      | 点击模型事件   |
| `iot/project/{projectId}/ui/point/click`      | 点击UI点位   |
| `iot/project/{projectId}/ui/point/mouseEnter` | 鼠标进入点位事件 |
| `iot/project/{projectId}/ui/point/mouseExit`  | 鼠标离开点位事件 |


####  场景控制指令  ####
# 主题格式                           #用途说明     |
| ----------------------------------------------------- | ------- |
| `iot/project/{projectId}/scene/points/showAll`        | 显示所有点位  |
{
	"MethodName": "ShowAllDeviceItemUI",
	"Data": null
}
| `iot/project/{projectId}/scene/points/hideAll`        | 隐藏所有点位  |
{
	"MethodName": "HideAllDeviceItemUI",
	"Data": null
}
| `iot/project/{projectId}/scene/points/lightUp`        | 点亮UI点位  |
{
	"MethodName": "CreateUIPoint",
	"Data": null
}
| `iot/project/{projectId}/scene/points/showAndLightUp` | 显示并点亮点位 |
{
	"MethodName": "GetAllDeviceInfo",
	"Data": [{
			"deviceId": "QX-01",
			"deviceColorState": "#112B7E"
		},
		{
			"deviceId": "QX-02",
			"deviceColorState": "#4EE8D0"
		}
	]
}
| `iot/project/{projectId}/scene/camera/reset`          | 相机复位    |
{
	"MethodName": "ResetCameraView",
	"Data": null
}

| `iot/project/{projectId}/scene/camera/focus`          | 相机聚焦到设备 |
{
	"MethodName": "SetCameraTarget",
	"Data": [{
			"deviceId": "QX-01",
			"deviceColorState": "#112B7E"
		}
	]
}


####  系统状态同步  ####
# 主题格式                           #用途说明     |
| ------------------------------------------------- | ----------- |
| `iot/project/{projectId}/system/unity/loadStatus` | Unity加载状态上报 |
| `iot/project/{projectId}/system/heartbeat`        | 系统心跳（双向）    |



#### 参与贡献
 

 

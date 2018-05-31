using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Vuforia;
using System;

public class Data : MonoBehaviour
{


    public Text nameObject;
    public Text infoName;
    public Button soundButton;
    public Button noSoundButton;
    public Button nameButton;
    public Button closeInfo;
    public Button openInfo;
    public GameObject infoPanel;

    private AudioSource audios;
    private AudioClip clips;
    public static Data instance;
    private String nameTrack;

    private List<string> names = new List<string> {"Con Bướm","Con Cá Voi","Con mèo","Con chim cánh cụt",
        "Con Voi", "Con Chim", "Con Nhện","Con Cá Mồi","Con ngựa", "Con Hổ","Con Tê Giác","Ốc Anh Vũ","Kì Nhông","Khỉ Đột","Khủng Long" };

    private List<string> infos = new List<string> {
"Bướm là các loài côn trùng nhỏ, biết bay, hoạt động vào ban ngày thuộc bộ Cánh vẩy ,có nhiều loại, ít màu cũng có mà sặc sỡ nhiều màu sắc cũng có. Thường chúng sống gần các bụi cây nhiều hoa để hút phấn hoa, mật hoa, góp phần trong việc giúp hoa thụ phấn.",
"Cá voi là các loài thú chủ yếu đã thích nghi đầy đủ với cuộc sống dưới nước. Cơ thể của chúng có dạng tựa hình thoi . Các chi trước bị biến đổi thành chân chèo. Các chi sau nhỏ là cơ quan vết tích, chúng không gắn vào xương sống và bị ẩn trong cơ thể. Đuôi có các thùy đuôi nằm ngang.",
"Mèo là những con vật có kỹ năng của thú săn mồi và được biết đến với khả năng săn bắt hàng nghìn loại sinh vật để làm thức ăn. Chúng đồng thời là những sinh vật thông minh, và có thể được dạy hay tự học cách sử dụng các công cụ đơn giản như mở tay nắm cửa hay giật nước trong nhà vệ sinh.",
"Chim cánh cụt là một bộ chim không cánh sinh sống dưới nước là chủ yếu tại khu vực Nam bán cầu, Châu Nam Cực chỉ toàn băng tuyết, với nhiệt độ trung bình hàng năm thấp nhất trong các châu lục trên Trái Đất. Tuy nhiên chim cánh cụt vẫn sống và có tới hàng chục loài khác nhau.",
"Voi là động vật có vú lớn nhất còn sinh sống trên mặt đất ngày nay. Các loài voi nhỏ nhất, với kích thước chỉ cỡ con bê hay con lợn lớn, là các loài voi tiền sử đã sinh sống trên đảo Crete cho tới khoảng năm 5000 TCN, và có thể là tới những năm khoảng 3000 TCN.",
"Các loài chim hiện đại mang các đặc điểm tiêu biểu như: có lông vũ, có mỏ và không răng, đẻ trứng có vỏ cứng, chỉ số trao đổi chất cao, tim có bốn ngăn, cùng với một bộ xương nhẹ nhưng chắc. Tất cả các loài chim đều có chi trước đã biển đổi thành cánh và hầu hết có thể bay.",
"Nhện là một bộ động vật săn mồi, không xương sống thuộc lớp hình nhện, cơ thể chỉ có hai phần, tám chân, miệng không hàm nhai, không cánh. Màng nhện được dùng làm nhiều việc như tạo dây để leo trèo trên vách, làm tổ trong hốc đá, tạo nơi giữ và gói mồi, giữ trứng và giữ tinh trùng.",
"Cá mồi là loại cá lớn thuộc họ Cá bạc má ,chủ yếu thuộc chi Thunnus, sinh sống ở vùng biển ấm, cách bờ độ 185 km trở ra. Ở Việt Nam, Cá ngừ đại dương là tên địa phương để chỉ loại cá ngừ mắt to và cá ngừ vây vàng. Cá ngừ đại dương là loại hải sản đặc biệt thơm ngon, mắt rất bổ, được chế biến thành nhiều loại món ăn ngon và tạo nguồn hàng xuất khẩu có giá trị.",
"Ngựa là một loài động vật có vú . Ngựa đã trải qua quá trình tiến hóa từ 45 đến 55 triệu năm để từ một dạng sinh vật nhỏ với chân nhiều ngón trở thành dạng động vật lớn với chân một ngón như ngày nay. Ngựa có tuổi thọ khoảng 25 đến 30 năm. Ngựa cái mang thai kéo dài khoảng 335-340 ngày. Ngựa thường sinh một.",
"Phần lớn các loài hổ sống trong rừng và đồng cỏ . Hổ đi săn đơn lẻ, thức ăn của chúng chủ yếu là các động vật ăn cỏ cỡ trung bình như hươu, nai, lợn rừng, trâu, v.v. Tuy nhiên chúng cũng có thể bắt các loại mồi cỡ to hay nhỏ hơn nếu hoàn cảnh cho phép.",
"Tê giác là các loài động vật nằm trong số 5 chi còn sống sót của động vật guốc lẻ trong họ Rhinocerotidae. Đặc trưng nổi bật của động vật có sừng này là lớp da bảo vệ của chúng được tạo thành từ các lớp chất keo với độ dày tối ưu khoảng 4 inch được sắp xếp theo cấu trúc mắt lưới.",
"Ốc anh vũ có chiếc vỏ cứng rất đẹp. Thân ốc mềm nằm trong vỏ, đối xứng 2 bên. Từ trung tâm vỏ óc ra đến miệng có những lớp màng ngăn chia vỏ thành hơn 30 buồng khí, cơ thể ốc chỉ chiếm một gian ngoài cùng, các gian còn lại đều bỏ trống. ",
"Kỳ nhông là loài bò sát, toàn thân phủ một lớp vảy. Chúng có cổ dài, đuôi và bộ chân khỏe, tứ chi phát triển. Hình hình dáng bên ngoài trông giống như con thạch sùng (thằn lằn) nhưng to và dài hơn. Có hai chân trước và hai chân sau, mỗi chân có 5 ngón tòe rộng, mặt dưới ngón có các nút bám để con vật dễ leo trèo.",
"Khỉ đột là một chi linh trưởng thuộc họ người, động vật ăn cỏ sống trong rừng rậm châu Phi, là giống lớn nhất trong bộ linh trưởng còn tồn tại. Khỉ đột thường sống dưới mặt đất, đi bằng bốn chân và chỉ đi bằng hai chân khi chuẩn bị đánh nhau. ",
"Khủng long là một nhóm đa dạng từ phân loại, hình thái đến sinh thái. Khủng long có mặt ở khắp các châu lục, qua những loài hiện còn cũng như những hóa thạch còn sót lại. Phần nhiều ăn cỏ, số khác ăn thịt. Tổ tiên của chúng là động vật hai chân. ",
 };



    void Start()
    {

        audios = gameObject.AddComponent<AudioSource>();
        if (instance == null)
            instance = this;
    }


    // Update is called once per frame
    void Update()
    {

        IEnumerable<TrackableBehaviour> tbs = TrackerManager.Instance.GetStateManager().GetActiveTrackableBehaviours();

        foreach (TrackableBehaviour tb in tbs)
        {
            nameTrack = tb.TrackableName;

            int index = Convert.ToInt32(nameTrack);
            nameObject.gameObject.GetComponent<Text>().text = names[index];

            if (nameTrack == "6" || nameTrack == "7" || nameTrack == "11" || nameTrack == "12")
            {
                _EnableSoundButton(false);
            }
            else
            {
                _EnableSoundButton(true);
                soundButton.GetComponent<Button>().onClick.AddListener(delegate { _LoadSound("Sound/" + nameTrack); });
            }

            infoName.GetComponent<Text>().text = infos[index];
            nameButton.GetComponent<Button>().onClick.AddListener(delegate { _LoadSound("NameSound/" + nameTrack); });

            openInfo.GetComponent<Button>().onClick.AddListener(delegate { _EnableInfo(false); });
            closeInfo.GetComponent<Button>().onClick.AddListener(delegate { _EnableInfo(true); });


        }


    }




    private void _LoadSound(string name)
    {
        clips = (AudioClip)Resources.Load(name);
        audios.clip = clips;
        audios.playOnAwake = false;
        audios.loop = false;
        audios.Play();
    }

    private void _EnableSoundButton(bool b)
    {
        if (b)
        {
            soundButton.gameObject.SetActive(true);
            noSoundButton.gameObject.SetActive(false);

        }
        else
        {
            soundButton.gameObject.SetActive(false);
            noSoundButton.gameObject.SetActive(true);

        }
    }

    private void _EnableInfo(bool b)
    {

        if (b)
        {
            openInfo.gameObject.SetActive(true);
            infoPanel.SetActive(false);
            closeInfo.gameObject.SetActive(false);
        }
        else
        {
            openInfo.gameObject.SetActive(false);
            infoPanel.gameObject.SetActive(true);
            closeInfo.gameObject.SetActive(true);
        }
    }

    public string _GetName()
    {
        return nameTrack;
    }
}

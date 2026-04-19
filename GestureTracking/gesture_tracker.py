import cv2
import mediapipe as mp
import math
import socket

mp_hands = mp.solutions.hands
mp_draw = mp.solutions.drawing_utils

hands = mp_hands.Hands(
    min_detection_confidence=0.7,
    min_tracking_confidence=0.7
)

sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
server_address = ("127.0.0.1", 5055)

cap = cv2.VideoCapture(0)

prev_x = None
prev_y = None

move_threshold = 0.015

while True:

    ret, frame = cap.read()

    if not ret:
        continue

    frame = cv2.flip(frame, 1)

    rgb = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)

    results = hands.process(rgb)

    if results.multi_hand_landmarks:

        for hand_landmarks, handedness in zip(
            results.multi_hand_landmarks,
            results.multi_handedness
        ):

            label = handedness.classification[0].label

            mp_draw.draw_landmarks(
                frame,
                hand_landmarks,
                mp_hands.HAND_CONNECTIONS
            )

            # ---------------- LEFT HAND (MORPH) ----------------

            if label == "Left":

                wrist = hand_landmarks.landmark[0]
                middle = hand_landmarks.landmark[9]

                dx = middle.x - wrist.x
                dy = middle.y - wrist.y

                angle = math.degrees(math.atan2(dy, dx))

                message = f"LEFT:{angle}"

                sock.sendto(message.encode(), server_address)

            # ---------------- RIGHT HAND ----------------

            if label == "Right":

                index_tip = hand_landmarks.landmark[8]
                middle_tip = hand_landmarks.landmark[12]
                middle_pip = hand_landmarks.landmark[10]
                thumb_tip = hand_landmarks.landmark[4]

                x = index_tip.x
                y = index_tip.y

                # -------- ribbon navigation --------

                if prev_x is not None:

                    dx = x - prev_x
                    dy = y - prev_y

                    if abs(dx) > move_threshold or abs(dy) > move_threshold:

                        message = f"RIGHT:{dx},{dy}"

                        sock.sendto(message.encode(), server_address)

                prev_x = x
                prev_y = y

                # -------- IMAGE OPEN GESTURE --------
                # Middle finger extended = open image

                middle_extended = middle_tip.y < middle_pip.y

                if middle_extended:
                    sock.sendto(b"IMAGE_OPEN", server_address)
                else:
                    sock.sendto(b"IMAGE_CLOSE", server_address)

                # -------- PINCH ZOOM --------

                pinch_index = math.sqrt(
                    (thumb_tip.x - index_tip.x) ** 2 +
                    (thumb_tip.y - index_tip.y) ** 2
                )

                pinch_middle = math.sqrt(
                    (thumb_tip.x - middle_tip.x) ** 2 +
                    (thumb_tip.y - middle_tip.y) ** 2
                )

                pinch_distance = min(pinch_index, pinch_middle)

                message = f"PINCH:{pinch_distance}"

                sock.sendto(message.encode(), server_address)

    else:

        prev_x = None
        prev_y = None

        sock.sendto(b"NO_HAND", server_address)

    cv2.imshow("Hand Tracker", frame)

    if cv2.waitKey(1) == 27:
        break

cap.release()
cv2.destroyAllWindows()
FROM python:3.4

RUN mkdir /code
WORKDIR /code
RUN echo "django >=1.9, <2" > /code/requirements.txt
RUN pip install -r requirements.txt
ADD . /code/

# ssh
ENV SSH_PASSWD "root:Docker!"
RUN apt-get update \
        && apt-get install -y --no-install-recommends dialog \
        && apt-get update \
    && apt-get install -y --no-install-recommends openssh-server \
    && echo "$SSH_PASSWD" | chpasswd 

RUN echo '#' >> /etc/ssh/sshd_config && \
    echo '# /etc/ssh/sshd_config' >> /etc/ssh/sshd_config && \
    echo '#' >> /etc/ssh/sshd_config && \
    echo 'Port  2222' >> /etc/ssh/sshd_config && \
    echo 'ListenAddress 0.0.0.0' >> /etc/ssh/sshd_config && \
    echo 'LoginGraceTime    180' >> /etc/ssh/sshd_config && \
    echo 'X11Forwarding yes' >> /etc/ssh/sshd_config && \
    echo 'Ciphers   aes128-cbc,3des-cbc,aes256-cbc' >> /etc/ssh/sshd_config && \
    echo 'MACs  hmac-sha1,hmac-sha1-96' >> /etc/ssh/sshd_config && \
    echo 'StrictModes   yes' >> /etc/ssh/sshd_config && \
    echo 'SyslogFacility    DAEMON' >> /etc/ssh/sshd_config && \
    echo 'PrintMotd     no' >> /etc/ssh/sshd_config && \
    echo 'IgnoreRhosts  no' >> /etc/ssh/sshd_config && \
    echo 'RhostsRSAAuthentication   yes' >> /etc/ssh/sshd_config && \
    echo 'RSAAuthentication no' >> /etc/ssh/sshd_config && \
    echo 'PasswordAuthentication    yes' >> /etc/ssh/sshd_config && \
    echo 'PermitEmptyPasswords  no' >> /etc/ssh/sshd_config && \
    echo 'PermitRootLogin   yes' >> /etc/ssh/sshd_config

RUN echo '#!/bin/bash' >> /usr/local/bin/init.sh && \
    echo 'set -e' >> /usr/local/bin/init.sh && \
    echo 'echo "Starting SSH ..."' >> /usr/local/bin/init.sh && \
    echo 'service ssh start' >> /usr/local/bin/init.sh && \
    echo 'python /code/manage.py runserver 0.0.0.0:8000' >> /usr/local/bin/init.sh && \
    chmod u+x /usr/local/bin/init.sh

EXPOSE 8000 2222
#CMD ["python", "/code/manage.py", "runserver", "0.0.0.0:8000"]
ENTRYPOINT ["init.sh"]